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
    private Transform muzzle;   //Transform representing end of barrel when fully extended (used to spawn muzzle effects)
    private Transform loadZone; //Transform representing position loaded cannonballs are placed in

    private AudioSource audioSource; //Cannon audio source component

    //Settings:
    [Header("Weapon Settings:")]
    [SerializeField] [Tooltip("How much force is imparted to cannonball when shooting")] private float shotForce;

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
    private CannonAmmo loadedAmmo;    //Object currently loaded into cannon (if any)
    private float timeUntilReady = 0; //Time (in seconds) before cannon is ready to fire again
    private Vector3 barrelOrigPos;    //Original local position of barrel
    private Vector3 barrelReciproPos; //Local barrel position when fully reciprocated (only needs to be computed once)

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
        if (loadedAmmo != null) return; //Only allow one cannonball to be loaded at a time
        if (timeUntilReady > 0) return; //Do not allow cannon to be loaded while in post-firing sequence

        //Handshake:
        ammo.IsLoaded(); //Indicate to cannonball that it has been loaded (causes hand to release cannonball)
        loadedAmmo = ammo; //Store reference to loaded cannonball

        //Place ammo in loaded position:
        loadedAmmo.transform.parent = transform;                       //Set cannon as parent of cannonBall
        loadedAmmo.transform.position = loadZone.position;             //Move cannonball to designated loaded position
        loadedAmmo.transform.localEulerAngles = new Vector3(90, 0, 0); //Align object with barrel (assuming objects are tallest along the Y axis)

        //Effects:
        audioSource.PlayOneShot(loadSound); //Play load sound
    }
    /// <summary>
    /// Fires currently-loaded ammo object out of cannon.
    /// </summary>
    private void Fire()
    {
        //Validity checks:
        if (loadedAmmo == null) //Dry-firing
        {
            audioSource.PlayOneShot(dryFireSound); //Play dry-fire sound
            return;                                //Prevent other firing logic from happening
        }

        //Prep cannonball:
        Physics.IgnoreCollision(loadedAmmo.coll, barrel.GetComponent<Collider>());   //Make sure cannonball can't hit barrel
        Physics.IgnoreCollision(loadedAmmo.coll, receiver.GetComponent<Collider>()); //Make sure cannonball can't hit receiver

        //Shoot cannonball:
        loadedAmmo.transform.parent = transform.root;                             //Unchild cannonball
        loadedAmmo.IsFired();                                                     //Indicate to cannonball that it has been fired
        loadedAmmo.rb.AddForce(transform.forward * shotForce, ForceMode.Impulse); //Shoot cannonball in direction cannon is facing according to shotforce

        //Effects:
        audioSource.PlayOneShot(fireSound); //Play firing sound
        timeUntilReady = barrelResetTime;   //Begin post-firing sequence

        //Cleanup:
        loadedAmmo = null; //Clear reference to loaded ammo
    }
}
