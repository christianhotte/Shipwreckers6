using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonAmmo : MonoBehaviour
{
    //Objects & Components:
    internal Rigidbody rb;       //Ammo rigidbody component
    internal Collider coll;      //Ammo collider component
    private Grabbable grabbable; //Component which optionally makes this ammo grabbable

    //Settings:
    [SerializeField] [Tooltip("Configuration determining ammo properties once fired")] private CannonAmmoConfig ammoProfile;

    //Runtime Memory Vars:
    private bool activeProjectile; //Enables cannonball to do damage as projectile

    //RUNTIME METHODS:
    private void Start()
    {
        //Get objects & components:
        if (TryGetComponent(out grabbable)) //Ammo can be grabbed by player (can just borrow stuff from Grabbable)
        {
            rb = grabbable.rb;     //Get rigidbody from grabbable component
            coll = grabbable.coll; //Get collider from grabbable component
        }
        else //Ammo cannot be grabbed by player (needs to find its own components)
        {
            if (!TryGetComponent(out rb)) { Debug.LogError(name + " needs a rigidbody to be grabbable"); Destroy(this); }  //Make sure object has a rigidbody
            if (!TryGetComponent(out coll)) { Debug.LogError(name + " needs a collider to be grabbable"); Destroy(this); } //Make sure object has a collider
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (activeProjectile) //Cannonball hits something while in projectile mode
        {
            print("Cannonball hit object: " + collision.gameObject.name);
        }
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Called when cannonball is loaded into cannon.
    /// </summary>
    public void IsLoaded()
    {
        //Release and lock cannonball:
        if (grabbable != null) //Special considerations for grabbable ammo
        {
            if (grabbable.currentHand != null) grabbable.currentHand.Release(); //Force hand to release cannonball (if applicable)
            grabbable.isGrabbable = false;                                      //Ensure cannonball can no longer be grabbed
        }
        rb.isKinematic = true; //Ensure cannonball is not affected by physics
    }
    /// <summary>
    /// Called when cannonball is fired from cannon.
    /// </summary>
    public void IsFired()
    {
        //Free cannonball:
        rb.isKinematic = false;  //Make cannonball affected by physics again
        activeProjectile = true; //Turn on projectile mode so that cannonball can do damage
    }
}