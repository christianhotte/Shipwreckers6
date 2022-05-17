using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR.Features.Interactions;

/// <summary>
/// Drives logic for loading and firing handheld cannon.
/// </summary>
public class Cannon : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Prefab References:
    [SerializeField] private GameObject aura; //Aura prefab to be attached to any projectile when fired

    //Objects & Components:
    private Transform barrel;   //Transform for barrel mesh object (used in reciprocation visual)
    private Transform receiver; //Transform for receiver mesh object
    private Transform muzzle;   //Transform representing end of barrel when fully extended (used to spawn shot projectiles and muzzle effects)
    private Transform loadZone; //Transform representing position loaded cannonballs are placed in
    private Transform needle;   //Transform for needle of gauge showing cannon charge state

    private AudioSource audioSource; //Cannon audio source component

    //Settings:
    [Header("Base Weapon Properties:")]
    [SerializeField] [Tooltip("How much force is imparted to cannonball when shooting")]                         private float shotForce;
    [SerializeField] [Tooltip("Multiplier used when converting cannon velocity to projectile angular velocity")] private float spinForce;

    [Header("Multishot Settings:")]
    [SerializeField] [Tooltip("Maximum number of things which may be loaded into cannon")]                                                                              private int maxAmmo;
    [SerializeField] [Tooltip("Radius of initial projectile distribution when firing multiple pieces of ammo")]                                                         private float initialMultishotOffset;
    [SerializeField] [Tooltip("Spread of multishot projectiles (value = degrees) depending on number (t) of fired projectiles, used to make multishots less accurate")] private AnimationCurve multishotSpreadAngle;
    [SerializeField] [Tooltip("Power multiplier (value) depending on number (t) of fired projectiles, used to make multishots weaker")]                                 private AnimationCurve multishotPowerFalloff;

    [Header("Charge Shot Settings:")]
    [SerializeField] [Tooltip("The longest amount of time player can hold down trigger when charging cannon")]              private float maxChargeTime;
    [SerializeField] [Tooltip("Maximum multiplier applied to shotforce when cannon is charged")]                            private float maxShotForceMult;
    [SerializeField] [Tooltip("Minimum multiplier applied to multishot spread when charging cannon")]                       private float minMultiSpreadMult;
    [SerializeField] [Tooltip("Maximum angular intensity of random position effect on needle (increases with charge time")] private float maxNeedleFlicker;

    [Header("Visuals & Sequence:")]
    [SerializeField] [Tooltip("Time taken after firing before cannon is ready to fire again")] private float barrelResetTime;
    [SerializeField] [Tooltip("How far back barrel reciprocates when firing")]                 private float barrelReciproDist;
    [SerializeField] [Tooltip("Curve describing linear motion of cannon barrel after firing")] private AnimationCurve barrelReciproCurve;
    [SerializeField] [Tooltip("Prefab containing temporary effects spawned in when firing")]   private GameObject muzzleEffectPrefab;

    [Header("Haptics:")]
    [SerializeField, Range(0, 1)] private float fireVibration;
    [SerializeField, Range(0, 1)] private float maxChargeVibration;
    [SerializeField, Range(0, 1)] private float loadVibration;

    [Header("Sounds:")]
    [SerializeField] [Tooltip("Sound made when cannon is loaded")]             private AudioClip loadSound;
    [SerializeField] [Tooltip("Sound made when cannon is fired")]              private AudioClip fireSound;
    [SerializeField] [Tooltip("Sound made when cannon is fired without ammo")] private AudioClip dryFireSound;

    //Runtime Memory Vars:
    private List<CannonAmmo> loadedAmmo = new List<CannonAmmo>(); //Object currently loaded into cannon (if any)
    private bool full = false;                                    //Whether or not cannon is currently full (and cannot accept any more ammo)

    private float timeUntilReady = 0; //Time (in seconds) before cannon is ready to fire again
    private Vector3 barrelOrigPos;    //Original local position of barrel
    private Vector3 barrelReciproPos; //Local barrel position when fully reciprocated (only needs to be computed once)

    private List<Vector3> velocityMem = new List<Vector3>(); //List of raw velocity vectors from last few physics updates (in order from latest to oldest)
    private Vector3 prevPosition;                            //Last position hand object was in, used to compute momentary velocity
    private float triggerHoldTime = -1;                      //Time player has been holding down the trigger (negative means trigger is not held)
    private const float maxNeedleAngle = 320;                //Maximum angle gauge needle can reach

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        barrel = transform.Find("Barrel"); if (barrel == null) { Debug.LogError("Cannon needs child named 'Barrel'"); Destroy(this); }            //Make sure cannon has barrel
        receiver = transform.Find("Receiver"); if (barrel == null) { Debug.LogError("Cannon needs child named 'Receiver'"); Destroy(this); }      //Make sure cannon has barrel
        muzzle = transform.Find("Muzzle"); if (muzzle == null) { Debug.LogError("Cannon needs child named 'Muzzle'"); Destroy(this); }            //Make sure cannon has muzzle
        loadZone = transform.Find("LoadZone"); if (loadZone == null) { Debug.LogError("Cannon needs child named 'LoadZone'"); Destroy(this); }    //Make sure cannon has load zone
        needle = transform.Find("Gauge").GetChild(0); if (needle == null) { Debug.LogError("Cannon needs child named 'Needle'"); Destroy(this); } //Make sure cannon has needle
        if (!TryGetComponent(out audioSource)) { Debug.LogError("Cannon is missing an audio source"); Destroy(this); }                            //Make sure cannon has audio source

        //Initialize runtime vars:
        prevPosition = transform.position;                                         //Get current position of cannon to start off
        velocityMem.Add(Vector3.zero);                                             //Set first velocity in memory to 0 (array needs at least one entry for safety)
        barrelOrigPos = barrel.localPosition;                                      //Store initial local position of barrel
        barrelReciproPos = barrelOrigPos; barrelReciproPos.z -= barrelReciproDist; //Compute target position of barrel when reciprocating (based on reciprocation distance)
    }
    private void Update()
    {
        //Update post-fire sequence:
        if (timeUntilReady != 0) //Post-firing sequence is currently active
        {
            //Update time tracker:
            timeUntilReady -= Time.deltaTime;              //Pass time
            timeUntilReady = Mathf.Max(timeUntilReady, 0); //Clamp time tracker to minimum of 0

            //Update barrel position:
            float reciprocationAmount = barrelReciproCurve.Evaluate(1 - timeUntilReady);               //Get interpolant value for how much to reciprocate barrel based on time remaining
            barrel.localPosition = Vector3.Lerp(barrelReciproPos, barrelOrigPos, reciprocationAmount); //Set new barrel position depending on reciprocation amount
        }

        //Update timers:
        if (triggerHoldTime >= 0) triggerHoldTime += Time.deltaTime; //Increment trigger hold time if player is holding down the trigger
        if (triggerHoldTime >= maxChargeTime) ForceFire();
    }
    private void FixedUpdate()
    {
        //Update current velocity:
        Vector3 currentVelocity = (transform.position - prevPosition) / Time.fixedDeltaTime; //Get velocity this physics update
        prevPosition = transform.position;                                                   //Update prevPosition once used
        velocityMem.Insert(0, currentVelocity);                                              //Insert latest velocity at beginning of memory list
        if (velocityMem.Count > 5) velocityMem.RemoveAt(5);                                  //Remove oldest item in memory if list is overfull

        //Charging Feedback:
        if (triggerHoldTime >= 0 && loadedAmmo.Count != 0) //Trigger is being held down
        {
            float chargeInterpolant = Mathf.Clamp01(triggerHoldTime / maxChargeTime); //Get interpolant value for current charge state

            //Haptics:
            float hapticIntensity = Mathf.Lerp(0, maxChargeVibration, chargeInterpolant); //Get haptic intensity based on trigger hold time
            SendHapticImpulse(hapticIntensity, 0.1f);                                     //Send haptic pulse (of arbitrary length) with strength based on current charge intensity

            //Gauge visual:
            float needleAngle = Mathf.Lerp(0, maxNeedleAngle, chargeInterpolant);                //Get needle angle based on trigger hold time
            float scaledNeedleFlicker = Mathf.Lerp(0, maxNeedleFlicker, chargeInterpolant);      //Get scaled maximum positional flicker for needle
            Vector3 newEulers = needle.localEulerAngles;                                         //Get local euler angles of needle object
            newEulers.z = needleAngle + Random.Range(-scaledNeedleFlicker, scaledNeedleFlicker); //Apply found angle to correct axis and add random noise to final needle position
            newEulers.z = Mathf.Clamp(newEulers.z, 0, 350);                                      //Clamp final needle angle to ensure it does not overflow
            needle.localEulerAngles = newEulers;                                                 //Set determined needle angle
        }
        else //Trigger is not being held down
        {
            if (needle.localEulerAngles.z != 0) //Needle position is not at zero
            {
                Vector3 needleEulers = needle.localEulerAngles;       //Get current local eulers of needle object
                needleEulers.z = Mathf.Lerp(needleEulers.z, 0, 0.2f); //Lerp needle toward zero position
                if (needleEulers.z <= 0.01f) needleEulers.z = 0;      //Snap to zero position if close enough
                needle.localEulerAngles = needleEulers;               //Set determined needle angle
            }
        }
    }
    public void OnFireInput(InputAction.CallbackContext context)
    {
        if (context.performed) //Trigger is pressed down
        {
            triggerHoldTime = 0; //Begin incrementation of triggerHoldTime
        }
        else if (context.canceled) //Trigger is released
        {
            ForceFire();
        }
            
    }
    public void ForceFire()
    {
        float t = Mathf.Min(triggerHoldTime, maxChargeTime) / maxChargeTime; //Generate interpolant value based on how long trigger has been held down
        float actualShotForce = Mathf.Lerp(1, maxShotForceMult, t);          //Determine shot force multiplier based on charge amount
        float spreadMultiplier = Mathf.Lerp(1, minMultiSpreadMult, t);       //Determine shot angle reducer based on charge amount
        Fire(actualShotForce, spreadMultiplier, false);                      //Fire cannon
        triggerHoldTime = -1;                                                //Indicate that trigger is no longer pressed
    }
    public void OnSingleFireInput(InputAction.CallbackContext context)
    {
        if (context.performed) Fire(true); //Perform shot immediately (no modified parameters)
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Loads ammo object into cannon.
    /// </summary>
    /// <param name="ammo">Controller for object being loaded into cannon.</param>
    public void Load(CannonAmmo ammo)
    {
        //Validity checks:
        if (full) return;                                             //Only allow ammo to be loaded if cannon has room
        if (timeUntilReady > 0) return;                               //Do not allow cannon to be loaded while in post-firing sequence
        if (ammo.ammoProfile.isLarge && loadedAmmo.Count > 0) return; //Do not allow large ammo to be loaded into cannon with other ammo

        //Handshake:
        ammo.IsLoaded();      //Indicate to ammo item that it has been loaded (causes hand to release ammunition)
        loadedAmmo.Add(ammo); //Store reference to loaded ammunition

        //Hide previous ammo (if applicable):
        if (loadedAmmo.Count > 1) //There is at least one ammo item loaded ahead of this one
        {
            loadedAmmo[loadedAmmo.Count - 2].GetComponentInChildren<MeshRenderer>().enabled = false; //Hide previous ammo item in cannon (may need futureproofing)
        }
        else //This is the first piece of ammunition being loaded into the cannon
        {
            if (triggerHoldTime > 0) triggerHoldTime = 0; //Reset trigger hold time if cannon has been getting charged while empty
        }

        //Place ammo in loaded position:
        ammo.transform.parent = transform;                       //Set cannon as parent of cannonBall
        ammo.transform.position = loadZone.position;             //Move cannonball to designated loaded position
        ammo.transform.localEulerAngles = new Vector3(90, 0, 0); //Align object with barrel (assuming objects are tallest along the Y axis)

        //Check if full:
        if (ammo.ammoProfile.isLarge) full = true;    //Indicate that cannon is full if loaded ammo is large
        if (loadedAmmo.Count >= maxAmmo) full = true; //Indicate that cannon is full if ammo capacity has been reached

        //Effects:
        audioSource.PlayOneShot(loadSound); //Play load sound
        SendHapticImpulse(loadVibration, 0.1f);
    }
    /// <summary>
    /// Fires all currently-loaded ammo out of cannon, applying given multipliers to shot properties.
    /// </summary>
    private void Fire(float forceMultiplier, float spreadMultiplier, bool singleShot)
    {
        //Validity checks:
        if (timeUntilReady > 0) return; //Cannon is not ready yet
        if (loadedAmmo.Count == 0) //Dry-firing
        {
            audioSource.PlayOneShot(dryFireSound); //Play dry-fire sound
            return;                                //Prevent other firing logic from happening
        }

        //Discharge all ammo items:
        Vector3 basePos = muzzle.localPosition;                                                              //Get shorthand for base initial position of fired projectiles (default is muzzle)
        float offsetRot = 360 / loadedAmmo.Count;                                                            //Get angle to rotate each ammo item around cannon center (in multishot scenario)
        float currentForce = shotForce * forceMultiplier * multishotPowerFalloff.Evaluate(loadedAmmo.Count); //Get actual shotforce by comparing number of fired projectiles to force falloff curve
        for (int i = 0; i < loadedAmmo.Count; i++) //Iterate through each individual ammo item in cannon
        {
            //Initialize:
            if (singleShot) { i = loadedAmmo.Count - 1; } //Only fire last projectile in stack if using singleshot mode
            CannonAmmo currentAmmo = loadedAmmo[i];       //Get reference to current ammo item
            Vector3 shotDirection = transform.forward;    //Direction to fire projectile (defaults to straight ahead but may be modified if using multishot)

            //Prep projectile:
            foreach (CannonAmmo otherAmmo in loadedAmmo) //Iterate through each loaded piece of ammunition in cannon
            {
                if (otherAmmo == currentAmmo) continue;                    //Make sure other piece of ammunition is not this one
                Physics.IgnoreCollision(currentAmmo.coll, otherAmmo.coll); //Make sure projectile can't hit other projectiles
            }
            Physics.IgnoreCollision(currentAmmo.coll, barrel.GetComponent<Collider>());   //Make sure projectile can't hit barrel
            Physics.IgnoreCollision(currentAmmo.coll, receiver.GetComponent<Collider>()); //Make sure projectile can't hit receiver
            currentAmmo.GetComponentInChildren<MeshRenderer>().enabled = true;            //Un-hide projectile mesh (may need futureproofing)

            //Position projectile:
            Vector3 newPosition = basePos; //Initialize container for new position (assume default base position)
            if (loadedAmmo.Count > 1 && !singleShot) //Positioning for multishot projectiles
            {
                //Circularly offset projectiles in multishot:
                float currentOffsetRot = (i * offsetRot) + (offsetRot / loadedAmmo.Count);                  //Get value for offset rotation (which determines characteristics of shot pattern)
                newPosition.x += initialMultishotOffset;                                                    //Apply designated multishot offset
                newPosition = Quaternion.Euler(0, 0, currentOffsetRot) * (newPosition - basePos) + basePos; //Get offset position rotated around center of barrel (arranges multishot projectiles in circular pattern)

                //Modify shotDirection to add spread:
                float spreadAngle = spreadMultiplier * multishotSpreadAngle.Evaluate(loadedAmmo.Count); //Get spread angle by evaluating fired projectile quantity
                shotDirection = Quaternion.Euler(spreadAngle, 0, 0) * Vector3.forward;                  //Apply rotation according to designated spread angle
                shotDirection = Quaternion.Euler(0, 0, Random.Range(0, 360)) * shotDirection;           //Apply random radial rotation (NEEDS REVISION, FEELS BAD)
                shotDirection = transform.localToWorldMatrix.MultiplyVector(shotDirection);             //Re-orient shot direction in world space
            }
            loadedAmmo[i].transform.localPosition = newPosition; //Set new position for ammo

            //Shoot projectile:
            currentAmmo.transform.parent = transform.root;                            //Unchild projectile
            currentAmmo.IsFired();                                                    //Indicate to projectile that it has been fired
            currentAmmo.rb.AddForce(shotDirection * currentForce, ForceMode.Impulse); //Shoot projectile in direction cannon is facing according to current shotforce

            //Add aura to projectile:
            if (aura != null)
            {
                GameObject newaura = Instantiate(aura);
                newaura.transform.localScale = Vector3.one/2.0f;
                newaura.transform.parent = currentAmmo.transform;
                newaura.transform.localPosition = Vector3.zero;
                newaura.name = "Aura";
            }

            //Haptics:
            SendHapticImpulse(fireVibration, 0.2f); //Send haptic impulse
        }

        //Cleanup:
        audioSource.PlayOneShot(fireSound); //Play firing sound
        timeUntilReady = barrelResetTime;   //Begin post-firing sequence
        full = false;                       //Indicate that cannon is no longer full
        if (singleShot) //Cleanup for singleshots
        {
            loadedAmmo.RemoveAt(loadedAmmo.Count - 1); //Just remove ammo at end of array
            if (loadedAmmo.Count > 0)                  //There is still some ammo in the cannon
            {
                loadedAmmo[loadedAmmo.Count - 1].GetComponentInChildren<MeshRenderer>().enabled = true; //Allow next piece of ammo in cannon to be seen
            }
        }
        else //Cleanup for multishots
        {
            loadedAmmo.Clear(); //Clear loaded ammo
        }
    }
    /// <summary>
    /// Fires all currently-loaded ammo out of cannon.
    /// </summary>
    /// <param name="singleShot"></param>
    private void Fire(bool singleShot)
    {
        Fire(1, 1, singleShot); //Call main method with default parameters
    }

    //UTILITY METHODS:
    /// <summary>
    /// Returns average of all stored velocities in recent memory (smoothed velocity).
    /// </summary>
    private Vector3 SmoothedCurrentVelocity()
    {
        Vector3 totalVelocity = Vector3.zero;                      //Initialize container for storing sum of velocity memory list
        foreach (Vector3 vel in velocityMem) totalVelocity += vel; //Add each velocity in memory to total velocity vector
        return totalVelocity / velocityMem.Count;                  //Return average velocity between all velocities in memory
    }
    /// <summary>
    /// Sends a haptic impulse to the right hand controller.
    /// </summary>
    /// <param name="amplitude">Strength of vibration (between 0 and 1).</param>
    /// <param name="duration">Duration of vibration (in seconds).</param>
    public void SendHapticImpulse(float amplitude, float duration)
    {
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();                   //Initialize list to store input devices
        UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.RightHanded, devices); //Find all input devices counted as right hand
        foreach (var device in devices) //Iterate through list of devices identified as right hand
        {
            if (device.TryGetHapticCapabilities(out UnityEngine.XR.HapticCapabilities capabilities)) //Device has haptic capabilities
            {
                if (capabilities.supportsImpulse) device.SendHapticImpulse(0, amplitude, duration); //Send impulse if supported by device
            }
        }
    }
}
