using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Allows player hand to grab objects.
/// </summary>
public class HandGrab : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    private Grabbable hoverObject; //Grabbable object hand is currently able to grab (if any)
    private Grabbable heldObject;  //Grabbable object currently being held by hand (if any)

    //Settings:
    [SerializeField] [Tooltip("Determines how quickly an object will orient itself when grabbed (when applicable)")] [Range(0, 1)] private float grabSnapStrength;

    //Runtime Memory Vars:
    private Vector3 prevPosition;    //Position of this controller last frame
    private Vector3 currentVelocity; //Current velocity of this controller

    //RUNTIME METHODS:
    private void Start()
    {
        //Initialize variables:
        prevPosition = transform.position; //Store current position
    }
    private void Update()
    {
        //Update current velocity:
        currentVelocity = (transform.position - prevPosition) / Time.deltaTime; //Update velocity tracker
        prevPosition = transform.position;                                      //Update previous position
    }
    private void FixedUpdate()
    {
        //Update held object orientation:
        if (heldObject != null) //Player is currently holding an object
        {
            if (heldObject.forceGrabPosition && heldObject.transform.localPosition != -heldObject.grabOrientation.localPosition) //Object needs to be lerped into position
            {
                //Lerp object toward hand orientation:
                Vector3 targetPosition = transform.position + (heldObject.transform.position - heldObject.grabOrientation.position); //Get target transform position
                Vector3 newPosition = Vector3.Lerp(heldObject.transform.position, targetPosition, grabSnapStrength);                 //Get new position by lerping with snap strength
                if (Vector3.Distance(targetPosition, newPosition) < 0.01) newPosition = targetPosition;                              //Just snap to target if close enoug
                heldObject.transform.position = newPosition;                                                                         //Apply new position to object
            }
            if (heldObject.forceGrabRotation && heldObject.transform.rotation != transform.rotation) //Object neewds to be lerped into rotation
            {
                //WORK IN PROGRESS
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //Try to get hovered object:
        if (heldObject != null || hoverObject != null) return;            //Ignore this if player is already holding or hovering over an object
        if (!other.TryGetComponent(out Grabbable grabController)) return; //Ignore this if object is not grabbable
        if (!grabController.isGrabbable) return;                          //Ignore if object is not grabbable
        hoverObject = grabController;                                     //Set this object as current object being hovered over
    }
    private void OnTriggerExit(Collider other)
    {
        //Try to remove hovered object:
        if (hoverObject == null) return;                        //Ignore if not currently hovering over an object
        if (other.gameObject != hoverObject.gameObject) return; //Ignore if exiting object is not the one which is currently being hovered over
        hoverObject = null;                                     //Remove hovered object reference
    }
    public void OnGrabInput(InputAction.CallbackContext context)
    {
        if (context.performed) //Grip squeezed
        {
            if (heldObject == null && hoverObject != null) Grab(); //Grab object if available
        }
        else //Grip released
        {
            if (heldObject != null) Release(); //Release held object if able
        }
        
    }

    //FUNCTIONALITY METHODS:
    private void Grab()
    {
        //Function: Grabs target grabbable

        //Validity checks:
        if (!hoverObject.isGrabbable) return; //Do not grab an ungrabbable object

        //Initialization:
        heldObject = hoverObject;   //Grab currently-hovered object
        heldObject.IsGrabbed(this); //Indicate to object that it has been grabbed

        //Move object:
        heldObject.transform.parent = transform;                   //Child object to hand
        if (heldObject.forceGrabPosition && grabSnapStrength == 1) //Case where object instantly snaps into hand position
        {
            heldObject.transform.position = transform.position + (heldObject.transform.position - heldObject.grabOrientation.position);
        }
        if (heldObject.forceGrabRotation && grabSnapStrength == 1) //Case where object instantly snaps into hand rotation
        {
            //WORK IN PROGRESS
        }
    }
    /// <summary>
    /// Releases currently-held object from hand.
    /// </summary>
    public void Release()
    {
        //Function: Releases currently-held grabbable

        //Validity checks:
        if (heldObject == null) return; //Make sure hand is holding an object

        //Cleanup:
        heldObject.transform.parent = transform.root; //Unchild held object
        heldObject.IsReleased(this);                  //Indicate that object has been released
        heldObject.rb.velocity = currentVelocity;     //Send current velocity
        heldObject = null;                            //Indicate object is no longer being held
    }
}
