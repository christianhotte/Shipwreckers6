using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special type of grabbable object which returns to specified holster when released.
/// </summary>
public class Holsterable : Grabbable
{
    //Classes, Enums & Structs:

    //Objects & Components:
    [SerializeField] [Tooltip("Location and orientation to snap to when released (generated at start if not given)")] private Transform holster;

    //Settings:
    [SerializeField] [Tooltip("How quickly object will return to holster once released")] private float snapStrength;

    //Runtime Memory Vars:
    internal bool isHolstered = true; //Whether or not this object is currently holstered

    //RUNTIME METHODS:
    public override void Awake()
    {
        //Initialize:
        base.Awake();          //Call base awake method
        rb.isKinematic = true; //Make sure rigidbody is in kinematic

        //Check holster status:
        if (holster == null) //No holster has been designated
        {
            GameObject holsterObject = new GameObject(name + "Holster"); //Generate empty object for new holster transform
            holster = holsterObject.transform;                           //Associate holster transform with generated object
            holster.parent = transform.parent;                           //Child holster to this object's parent
            holster.position = transform.position;                       //Save initial position
            holster.rotation = transform.rotation;                       //Save initial rotation
        }
        else //Object already has a designated holster
        {
            transform.position = holster.position; //Set object to holstered position
            transform.rotation = holster.rotation; //Set object to holstered rotation
        }
    }
    private void FixedUpdate()
    {
        if (!isHolstered && !beingGrabbed) //Object is not currently holstered or held
        {
            //Find new position and rotation:
            Vector3 newPos = Vector3.Lerp(transform.position, holster.position, snapStrength);        //Get new position for object
            Quaternion newRot = Quaternion.Slerp(transform.rotation, holster.rotation, snapStrength); //Get new rotation for object
            if (Vector3.Distance(newPos, holster.position) < 0.01) //Object is close enough to holster
            {
                newPos = holster.position; //Snap to position
                newRot = holster.rotation; //Snap to rotation
                IsHolstered();             //Indicate that object is now holstered
            }

            //Set new transform properties
            transform.position = newPos; //Set new position
            transform.rotation = newRot; //Set new rotation
        }
    }

    //FUNCTIONALITY METHODS:
    public override void IsGrabbed(HandGrab controller)
    {
        base.IsGrabbed(controller); //Call base grab method
        foreach (Animator anim in Fingerer.main.anims) anim.SetInteger("GrabType", 1);
        isHolstered = false;        //Indicate that object is no longer holstered
    }
    public override void IsReleased(HandGrab controller)
    {
        base.IsReleased(controller); //Call base release method
        rb.isKinematic = true;       //Re-set object to kinematic while traveling to holster
        isGrabbable = false;         //Prevent object from being grabbed while traveling to holster
    }
    private void IsHolstered()
    {
        //Function: Called when this object reaches its holster

        transform.parent = holster.parent; //Reparent object to original parent
        isHolstered = true; //Indicate that object is now holstered
        isGrabbable = true; //Indicate that object may now be grabbed
    }
}
