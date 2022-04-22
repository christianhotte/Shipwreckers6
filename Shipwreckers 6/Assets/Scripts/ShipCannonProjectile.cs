using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCannonProjectile : MonoBehaviour
{
    //Objects & Components:
    /// <summary>Position shot projectile will home toward.</summary>
    internal Transform target;
    private Rigidbody rb; //This projectile's rigidbody component

    //Settings:
    [Header("Homing Settings:")]
    [SerializeField] [Tooltip("How many degrees per second projectile is able to home in by")] private float homingStrength;
    [SerializeField] [Tooltip("Distance to target at which projectile will stop homing in")] private float homingEndDist;

    [Header("Projectile Settings:")]
    [SerializeField] [Tooltip("Prefab spawned when object is cut by sword, object cannot be cut if left null")] private GameObject cutPrefab;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        rb = GetComponent<Rigidbody>(); //Get rigidbody component
    }
    private void FixedUpdate()
    {
        //Projectile homing:
        if (Vector3.Distance(target.position, transform.position) > homingEndDist) //Projectile is far enough away to keep homing in
        {
            Vector3 currentVel = rb.velocity;                                                               //Get projectile's current velocity
            Vector3 targetVel = (target.position - transform.position).normalized * currentVel.magnitude;   //Get velocity which would point projectile directly at target
            rb.velocity = Vector3.MoveTowards(currentVel, targetVel, homingStrength * Time.fixedDeltaTime); //Modify velocity to make projectile point more toward target
        }
        else if (!rb.useGravity) rb.useGravity = true; //Enable gravity once projectile is close enough to target
    }
    private void OnTriggerEnter(Collider collider)
    {
        //Validity checks:
        if (collider.GetComponent<ShipCannonProjectile>() != null) return; //Ignore if projectile hit another projectile from same broadside

        //Projectile strike procedure:
        if (collider.CompareTag("Player")) PlayerHealthManager.HurtPlayer(1); //Hurt player if hit

        //Cleanup:
        Destroy(gameObject); //Destroy gameobject after it hits a thing
    }
}
