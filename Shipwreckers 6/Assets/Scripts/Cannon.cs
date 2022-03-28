using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Drives logic for loading and firing handheld cannon.
/// </summary>
public class Cannon : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    private Transform barrel;   //Transform for barrel mesh object (used in reciprocation visual)
    private Transform receiver; //Transform for receiver mesh object
    private Transform muzzle;   //Transform representing end of barrel when fully extended (used to spawn shot projectiles and muzzle effects)
    private Transform loadZone; //Transform representing position loaded cannonballs are placed in

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

    [Header("Visuals & Sequence:")]
    [SerializeField] [Tooltip("Time taken after firing before cannon is ready to fire again")] private float barrelResetTime;
    [SerializeField] [Tooltip("How far back barrel reciprocates when firing")]                 private float barrelReciproDist;
    [SerializeField] [Tooltip("Curve describing linear motion of cannon barrel after firing")] private AnimationCurve barrelReciproCurve;
    [SerializeField] [Tooltip("Prefab containing temporary effects spawned in when firing")]   private GameObject muzzleEffectPrefab;

    [Header("Sounds:")]
    [SerializeField] [Tooltip("Sound made when cannon is loaded")]             private AudioClip loadSound;
    [SerializeField] [Tooltip("Sound made when cannon is fired")]              private AudioClip fireSound;
    [SerializeField] [Tooltip("Sound made when cannon is fired without ammo")] private AudioClip dryFireSound;

    //Runtime Memory Vars:
    private List<CannonAmmo> loadedAmmo = new List<CannonAmmo>(); //Object currently loaded into cannon (if any)

    private float timeUntilReady = 0; //Time (in seconds) before cannon is ready to fire again
    private Vector3 barrelOrigPos;    //Original local position of barrel
    private Vector3 barrelReciproPos; //Local barrel position when fully reciprocated (only needs to be computed once)

    private List<Vector3> velocityMem = new List<Vector3>(); //List of raw velocity vectors from last few physics updates (in order from latest to oldest)
    private Vector3 prevPosition;                            //Last position hand object was in, used to compute momentary velocity

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        barrel = transform.Find("Barrel"); if (barrel == null) { Debug.LogError("Cannon needs child named 'Barrel'"); Destroy(this); }         //Make sure cannon has barrel
        receiver = transform.Find("Receiver"); if (barrel == null) { Debug.LogError("Cannon needs child named 'Receiver'"); Destroy(this); }   //Make sure cannon has barrel
        muzzle = transform.Find("Muzzle"); if (muzzle == null) { Debug.LogError("Cannon needs child named 'Muzzle'"); Destroy(this); }         //Make sure cannon has muzzle
        loadZone = transform.Find("LoadZone"); if (loadZone == null) { Debug.LogError("Cannon needs child named 'LoadZone'"); Destroy(this); } //Make sure cannon has load zone
        if (!TryGetComponent(out audioSource)) { Debug.LogError("Cannon is missing an audio source"); Destroy(this); }                         //Make sure cannon has audio source

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
    }
    private void FixedUpdate()
    {
        //Update current velocity:
        Vector3 currentVelocity = (transform.position - prevPosition) / Time.fixedDeltaTime; //Get velocity this physics update
        prevPosition = transform.position;                                                   //Update prevPosition once used
        velocityMem.Insert(0, currentVelocity);                                              //Insert latest velocity at beginning of memory list
        if (velocityMem.Count > 5) velocityMem.RemoveAt(5);                                  //Remove oldest item in memory if list is overfull
    }
    public void OnFireInput(InputAction.CallbackContext context)
    {
        if (context.performed) Fire(); //Fire cannon when fire input is performed
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Loads ammo object into cannon.
    /// </summary>
    /// <param name="ammo">Controller for object being loaded into cannon.</param>
    public void Load(CannonAmmo ammo)
    {
        //Validity checks:
        if (loadedAmmo.Count >= maxAmmo) return; //Only allow ammo to be loaded if cannon has room
        if (timeUntilReady > 0) return;          //Do not allow cannon to be loaded while in post-firing sequence

        //Handshake:
        ammo.IsLoaded();      //Indicate to cannonball that it has been loaded (causes hand to release ammunition)
        loadedAmmo.Add(ammo); //Store reference to loaded ammunition

        //Hide previous ammo (if applicable):
        if (loadedAmmo.Count > 1) //There is at least one ammo item loaded ahead of this one
        {
            loadedAmmo[loadedAmmo.Count - 2].GetComponentInChildren<MeshRenderer>().enabled = false; //Hide previous ammo item in cannon (may need futureproofing)
        }

        //Place ammo in loaded position:
        ammo.transform.parent = transform;                       //Set cannon as parent of cannonBall
        ammo.transform.position = loadZone.position;             //Move cannonball to designated loaded position
        ammo.transform.localEulerAngles = new Vector3(90, 0, 0); //Align object with barrel (assuming objects are tallest along the Y axis)

        //Effects:
        audioSource.PlayOneShot(loadSound); //Play load sound
    }
    /// <summary>
    /// Fires all currently-loaded ammo out of cannon.
    /// </summary>
    private void Fire()
    {
        //Validity checks:
        if (loadedAmmo.Count == 0) //Dry-firing
        {
            audioSource.PlayOneShot(dryFireSound); //Play dry-fire sound
            return;                                //Prevent other firing logic from happening
        }

        //Discharge all ammo items:
        Vector3 basePos = muzzle.localPosition;                                            //Get shorthand for base initial position of fired projectiles (default is muzzle)
        float offsetRot = 360 / loadedAmmo.Count;                                          //Get angle to rotate each ammo item around cannon center (in multishot scenario)
        float currentForce = shotForce * multishotPowerFalloff.Evaluate(loadedAmmo.Count); //Get actual shotforce by comparing number of fired projectiles to force falloff curve
        for (int i = 0; i < loadedAmmo.Count; i++) //Iterate through each individual ammo item in cannon
        {
            //Initialize:
            CannonAmmo currentAmmo = loadedAmmo[i];    //Get reference to current ammo item
            Vector3 shotDirection = transform.forward; //Direction to fire projectile (defaults to straight ahead but may be modified if using multishot)

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
            if (loadedAmmo.Count > 1) //Positioning for multishot projectiles
            {
                //Circularly offset projectiles in multishot:
                float currentOffsetRot = (i * offsetRot) + (offsetRot / loadedAmmo.Count);                  //Get value for offset rotation (which determines characteristics of shot pattern)
                newPosition.x += initialMultishotOffset;                                                    //Apply designated multishot offset
                newPosition = Quaternion.Euler(0, 0, currentOffsetRot) * (newPosition - basePos) + basePos; //Get offset position rotated around center of barrel (arranges multishot projectiles in circular pattern)

                //Modify shotDirection to add spread:
                float spreadAngle = multishotSpreadAngle.Evaluate(loadedAmmo.Count);          //Get spread angle by evaluating fired projectile quantity
                shotDirection = Quaternion.Euler(spreadAngle, 0, 0) * Vector3.forward;        //Apply rotation according to designated spread angle
                shotDirection = Quaternion.Euler(0, 0, Random.Range(0, 360)) * shotDirection; //Apply random radial rotation (NEEDS REVISION, FEELS BAD)
                shotDirection = transform.localToWorldMatrix.MultiplyVector(shotDirection);   //Re-orient shot direction in world space
            }
            loadedAmmo[i].transform.localPosition = newPosition; //Set new position for ammo

            //Shoot projectile:
            currentAmmo.transform.parent = transform.root;                            //Unchild projectile
            currentAmmo.IsFired();                                                    //Indicate to projectile that it has been fired
            currentAmmo.rb.AddForce(shotDirection * currentForce, ForceMode.Impulse); //Shoot projectile in direction cannon is facing according to current shotforce
        }

        //Cleanup:
        loadedAmmo.Clear();                 //Clear loaded ammo list
        audioSource.PlayOneShot(fireSound); //Play firing sound
        timeUntilReady = barrelResetTime;   //Begin post-firing sequence
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
}
