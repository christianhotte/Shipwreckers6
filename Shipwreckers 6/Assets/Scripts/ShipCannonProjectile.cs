using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCannonProjectile : MonoBehaviour
{
    //Objects & Components:
    /// <summary>Position shot cannonball will home toward.</summary>
    internal Transform target;

    //Settings:
    [Header("Homing Settings:")]
    [SerializeField] [Tooltip("How many degrees per second projectile is able to home in by")] private float homingStrength;
    [SerializeField] [Tooltip("Distance to target at which projectile will stop homing in")] private float homingEndDist;

    //RUNTIME METHODS:
    private void FixedUpdate()
    {
        //Projectile homing:
        if (Vector3.Distance(target.position, transform.position) > homingEndDist) //Projectile is far enough away to keep homing in
        {
            Vector3 currentVel = GetComponent<Rigidbody>().velocity;                                                               //Get projectile's current velocity
            Vector3 targetVel = (target.position - transform.position).normalized * currentVel.magnitude;                          //Get velocity which would point projectile directly at target
            GetComponent<Rigidbody>().velocity = Vector3.MoveTowards(currentVel, targetVel, homingStrength * Time.fixedDeltaTime); //Modify velocity to make projectile point more toward target
        }
    }
}
