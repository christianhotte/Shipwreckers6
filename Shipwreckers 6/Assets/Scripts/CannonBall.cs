using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : Grabbable
{
    //Runtime Memory Vars:


    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Called when cannonball is loaded into cannon.
    /// </summary>
    public void Load()
    {
        //Setup:
        if (currentHand != null) currentHand.Release(); //Force hand to release cannonball (if applicable)
        rb.isKinematic = true; //Ensure cannonball is not affected by physics
        isGrabbable = false;   //Ensure cannonball can no longer be grabbed
    }
}
