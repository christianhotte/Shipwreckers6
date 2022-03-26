using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : Grabbable
{
    //Settings:

    //Runtime Memory Vars:
    private bool activeProjectile; //Enables cannonball to do damage as projectile

    //RUNTIME METHODS:
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
        if (currentHand != null) currentHand.Release(); //Force hand to release cannonball (if applicable)
        rb.isKinematic = true; //Ensure cannonball is not affected by physics
        isGrabbable = false;   //Ensure cannonball can no longer be grabbed
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
