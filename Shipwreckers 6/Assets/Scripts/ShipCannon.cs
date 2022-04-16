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

    //Settings:
    [SerializeField] [Tooltip("Prefab for projectile object fired by this cannon")] private GameObject projectile;
    [SerializeField] [Tooltip("Velocity at which cannon shoots projectiles")] private float shootSpeed;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        allCannons.Add(this); //Add this cannon to cannon list upon creation

        //Get objects & components:
        barrelEnd = transform.Find("BarrelEnd"); //Get barrel end transform
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
        return true; //NOTE: Add stuff here if necessary
    }
    /// <summary>
    /// Triggers cannon firing sequence.
    /// </summary>
    /// <returns>Returns fired projectile.</returns>
    public GameObject FireAtTarget(Transform target)
    {
        //Generate projectile:
        GameObject newProjectile = Instantiate(projectile, barrelEnd.position, barrelEnd.transform.rotation);              //Generate a new cannonball
        newProjectile.GetComponent<Rigidbody>().velocity = (target.position - barrelEnd.position).normalized * shootSpeed; //Set projectile velocity to fire at player
        return newProjectile;
    }
    /// <summary>
    /// Permanently deploys cannon from stowed position.
    /// </summary>
    public void Deploy()
    {

    }
}
