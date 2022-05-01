using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCannonProjectile : MonoBehaviour
{
    //Objects & Components:
    /// <summary>Position shot projectile will home toward.</summary>
    internal Vector3 target;
    private Rigidbody rb;      //This projectile's rigidbody component
    private float startDistFromTarget; //Distance from target set on awake
    private float currentDistFromTarget; //Distance from target set every frame
    internal float startSpeed; //Speed at the start
    internal float endSpeed; //Speed at the end
    private float currentSpeed; //Calculated speed right now
    private bool stillHoming; //Is the projectile homing?

    //Settings:
    [Header("Homing Settings:")]
    [SerializeField] [Tooltip("How many degrees per second projectile is able to home in by")] internal float homingStrength;
    [SerializeField] [Tooltip("Distance to target at which projectile will stop homing in")] internal float homingEndDist;

    [Header("Projectile Settings:")]
    [SerializeField] [Tooltip("Opject enabled when projectile is cut by sword, object cannot be cut if left null")] private GameObject cutObject;
    [SerializeField] [Tooltip("How long (in seconds) cut object lasts after being spawned")] private float cutObjectLife;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        rb = GetComponent<Rigidbody>(); //Get rigidbody component
        startDistFromTarget = Vector3.Distance(target, transform.position);
        stillHoming = true;
    }
    private void FixedUpdate()
    {
        currentDistFromTarget = Vector3.Distance(target, transform.position);
        currentSpeed = Mathf.Lerp(startSpeed, endSpeed, 1.0f-(currentDistFromTarget/startDistFromTarget) );
        //currentSpeed = startSpeed;
        //Projectile homing:
        if (currentDistFromTarget > homingEndDist && stillHoming) //Projectile is far enough away to keep homing in
        {
            Vector3 currentVel = rb.velocity;                                                               //Get projectile's current velocity
            Vector3 targetVel = (target - transform.position).normalized * currentSpeed;   //Get velocity which would point projectile directly at target
            rb.velocity = Vector3.Lerp(currentVel, targetVel, homingStrength * Time.fixedDeltaTime); //Modify velocity to make projectile point more toward target
        }
        else if (!rb.useGravity)
        {
            // rb.useGravity = true; //Enable gravity once projectile is close enough to target
            stillHoming = false;
        }
    }
    private void OnTriggerEnter(Collider collider)
    {
        //Validity checks:
        if (collider.GetComponent<ShipCannonProjectile>() != null) return; //Ignore if projectile hit another projectile from same broadside

        //Projectile strike procedure:
        if (collider.CompareTag("Player")) PlayerHealthManager.HurtPlayer(1); //Hurt player if hit
        if (collider.GetComponent<SwordDriver>()) collider.GetComponent<SwordDriver>().CutProcedure(this); //Try to have sword cut this projectile

        //Cleanup:
        Destroy(gameObject); //Destroy gameobject after it hits a thing
    }

    //PUBLIC METHODS:
    /// <summary>
    /// Attempts to cut this projectile in half in given direction.
    /// </summary>
    /// <param name="direction">Direction in which to cut projectile</param>
    /// <param name="separationForce">How much force to add to separate cannonball halves</param>
    public bool AttemptToCut(Vector3 direction, float separationForce)
    {
        //Validity checks:
        if (cutObject == null) return false; //Do not attempt cut if object cannot be cut
        print("oog");
        //Swap projectile for cut object:
        cutObject.transform.parent = null;                                       //Unchild cutObject from projectile
        cutObject.SetActive(true);                                               //Activate cutObject
        cutObject.transform.rotation = Quaternion.LookRotation(Vector3.forward); //Rotate cutObject to align with blade
        for (int i = 0; i < cutObject.transform.childCount; i++) //Iterate through children of cut object
        {
            //Initialize:
            Rigidbody childRB = cutObject.transform.GetChild(i).GetComponent<Rigidbody>(); //Get child rigidbody component
            childRB.velocity = rb.velocity * 0.2f; //Copy velocity to child

            //Determine separation velocity
            Vector3 sepVel = new Vector3(); //Initialize vector for separating cannonball halves
            if (i == 0) sepVel = (cutObject.transform.GetChild(0).position - cutObject.transform.GetChild(1).position).normalized * separationForce;
            else sepVel = (cutObject.transform.GetChild(1).position - cutObject.transform.GetChild(0).position).normalized * separationForce;
            childRB.AddForce(sepVel); //Add found separation velocity to halves
        }

        //Cleanup:
        Destroy(cutObject, cutObjectLife); //Destroy cannonBall bits after a certain period of time
        gameObject.SetActive(false);       //De-activate this gameObject (to prevent collision and rendering)
        Destroy(gameObject, 0.1f);         //Destroy original instance of this object (whenever, time is arbitrary)
        return true;                       //Indicate that object was successfully cut
    }
}
