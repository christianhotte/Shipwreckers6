using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCannon : MonoBehaviour
{
    //Objects & Components:
    /// <summary>Master list of all ship cannons loaded in scene.</summary>
    public static List<ShipCannon> allCannons = new List<ShipCannon>();
    /// <summary>Transform representing the position and direction of the end of this cannon's barrel.</summary>
    internal Transform barrelEnd;
    private AudioSource audioSource;

    //Settings:
    [SerializeField] [Tooltip("Prefab for projectile object fired by this cannon")] private GameObject projectile;
    [SerializeField] [Tooltip("Prefab for smoke puff when projectile is fired")] private GameObject smokepuff;
    [SerializeField] [Tooltip("Sound cannon makes when fired")] private AudioClip shootSound;
    [Space()]
    [SerializeField] [Tooltip("Velocity at which cannon shoots projectiles")] private float shootSpeed;

    //Runtime vars:
    private float fireWaitTime;        //Time (in seconds) before cannon fires
    private Transform scheduledTarget; //Target (in world space) cannon is about to fire at

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (projectile.GetComponent<ShipCannonProjectile>() == null) Debug.LogError("Cannon projectile is missing ShipCannonProjectile script");

        //Get objects & components:
        barrelEnd = transform.Find("BarrelEnd"); //Get barrel end transform
        audioSource = GetComponent<AudioSource>(); //Get audio source component
    }
    private void Update()
    {
        if (fireWaitTime > 0)
        {
            fireWaitTime -= Time.deltaTime;
            if (fireWaitTime <= 0)
            {
                FireAtTarget(scheduledTarget);
                fireWaitTime = 0;
            }
        }
    }

    //PUBLIC METHODS:
    /// <summary>
    /// Whether or not cannon is within given degrees of target.
    /// </summary>
    /// <param name="target">Target transform.</param>
    /// <param name="maxDegrees">Maximum number of degrees away from target cannon can be facing.</param>
    /// <returns></returns>
    public bool IsFacingTarget(Transform target, float maxDegrees)
    {
        Vector3 directionToTarget = (target.position - barrelEnd.position).normalized;          //Get direction between cannon barrel end and target
        Vector3 cannonFacingDirection = barrelEnd.forward;                                      //Get direction cannon barrel is facing
        if (Vector3.Angle(directionToTarget, cannonFacingDirection) > maxDegrees) return false; //Indicate that cannon is too far from target
        return true;                                                                            //Indicate that target is within given degree area
    }
    /// <summary>
    /// Triggers cannon firing sequence.
    /// </summary>
    /// <param name="waitTime">Time for cannon to wait before firing.</param>
    public void FireAtTarget(Transform target, float waitTime, float startShootSpeed, float endShootSpeed, Vector3 inaccuracy, float homingStrength, float homingEndDist)
    {
        //Delayed fire protocol:
        if (waitTime > 0) //Cannon is being scheduled to fire
        {
            fireWaitTime = waitTime;  //Schedule shot
            scheduledTarget = target; //Indicate future target to fire at
            return;                   //Don't shoot yet
        }

        //Generate projectile:
        GameObject newProjectile = Instantiate(projectile, barrelEnd.position, barrelEnd.transform.rotation); //Generate a new cannonball
        newProjectile.GetComponent<Rigidbody>().velocity = barrelEnd.forward * startShootSpeed;                    //Set projectile velocity to fire at player

        //Setup projectile:
        Vector3 offset = new Vector3(
                Random.Range(-inaccuracy.x, inaccuracy.x),
                Random.Range(-inaccuracy.y, inaccuracy.y),
                Random.Range(-inaccuracy.z, inaccuracy.z)
            );
        ShipCannonProjectile projScript = newProjectile.GetComponent<ShipCannonProjectile>();
        projScript.target = target.position + offset; //Indicate to projectile where its target is
        projScript.startSpeed = startShootSpeed;
        projScript.endSpeed = endShootSpeed;
        if (homingStrength > 0) projScript.homingStrength = homingStrength;
        if (homingEndDist > 0) projScript.homingEndDist = homingEndDist;
        

        //Effects:
        audioSource.PlayOneShot(shootSound); //Play cannon firing sound
        GameObject newSmoke = Instantiate(smokepuff);
        newSmoke.transform.position = barrelEnd.position;
        Destroy(newSmoke, 5.0f);
    }
    public void FireAtTarget(Transform target)
    {
        FireAtTarget(target, 0, Vector3.Distance(transform.position, target.position), 4, new Vector3(0.1f, 0.0f, 0.1f), Vector3.Distance(transform.position, target.position)*0.1f, 5);
    }
    /// <summary>
    /// Fires all cannons pointing vaguely toward target at target.
    /// </summary>
    /// <param name="target">Target to fire cannons at.</param>
    /// <param name="maxDegrees">Number used to exclude cannons which are angled too far away from target to fire.</param>
    /// <param name="sequenceTime">Time taken between first and last cannons firing.</param>
    public static void FireAllCannonsAtTarget(Transform target, float maxDegrees, float sequenceTime)
    {
        //Find cannons actually being fired
        List<ShipCannon> firingCannons = new List<ShipCannon>(); //List of all cannons actually being fired
        foreach (ShipCannon cannon in allCannons)
        {
            if (cannon.IsFacingTarget(target, maxDegrees)) firingCannons.Add(cannon); //Add cannon if it is facing target
        }

        //Fire cannons at set intervals:
        if (firingCannons.Count == 0) return; //Ignore if no cannons are firing
        float interval = sequenceTime / firingCannons.Count; //Find interval (in seconds) between each cannon firing
        for (int i = 0; i < firingCannons.Count; i++) //Iterate through each cannon in firing list
        {
            firingCannons[i].FireAtTarget(target); //Fire each cannon at set interval
        }
    }
    /// <summary>
    /// Permanently deploys cannon from stowed position.
    /// </summary>
    public void Deploy()
    {

    }
    /// <summary>
    /// Picks a random cannon and fires it
    /// </summary>
    public static void FireRandomCannon(Transform target, float maxDegrees)
    {
        //Find cannons which may be fired
        List<ShipCannon> possibleCannons = new List<ShipCannon>(); //List of all cannons actually being fired
        foreach (ShipCannon cannon in allCannons)
        {
            if (cannon.IsFacingTarget(target, maxDegrees)) possibleCannons.Add(cannon); //Add cannon if it is facing target
        }
        if (possibleCannons.Count < 1) return;
        //Pick one of the cannons
        int choice = Random.Range( 0, possibleCannons.Count-1 );
        ShipCannon selected = possibleCannons[choice];
        //Fire
        selected.FireAtTarget(target); //Fire each cannon at set interval
    }
    private void OnEnable()
    {
        allCannons.Add(this); //Add this cannon to cannon list upon being enabled
    }
    private void OnDisable()
    {
        allCannons.Remove(this); //Remove this cannon from the list upon being disabled
    }
}
