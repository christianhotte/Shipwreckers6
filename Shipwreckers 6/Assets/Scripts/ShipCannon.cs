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
        allCannons.Add(this); //Add this cannon to cannon list upon creation
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
                FireAtTarget(scheduledTarget, 0);
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
    public void FireAtTarget(Transform target, float waitTime)
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
        newProjectile.GetComponent<Rigidbody>().velocity = barrelEnd.forward * shootSpeed;                    //Set projectile velocity to fire at player

        //Setup projectile:
        newProjectile.GetComponent<ShipCannonProjectile>().target = target; //Indicate to projectile where its target is

        //Effects:
        audioSource.PlayOneShot(shootSound); //Play cannon firing sound
        GameObject newSmoke = Instantiate(smokepuff);
        newSmoke.transform.position = barrelEnd.position;
        Destroy(newSmoke, 5.0f);
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
            firingCannons[i].FireAtTarget(target, interval * i); //Fire each cannon at set interval
        }
    }
    /// <summary>
    /// Permanently deploys cannon from stowed position.
    /// </summary>
    public void Deploy()
    {

    }
}
