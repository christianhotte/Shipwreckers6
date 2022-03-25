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


    //RUNTIME METHODS:
    private void OnTriggerEnter(Collider other)
    {
        //Try to get hovered object:
        if (heldObject != null || hoverObject != null) return;            //Ignore this if player is already holding or hovering over an object
        if (!other.TryGetComponent(out Grabbable grabController)) return; //Ignore this if object is not grabbable
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

        //Initialization:
        heldObject = hoverObject; //Grab currently-hovered object
        heldObject.IsGrabbed();   //Indicate to object that it has been grabbed

        //Move object:
        heldObject.transform.parent = transform;                   //Child object to hand
        if (heldObject.forceGrabPosition && grabSnapStrength == 1) //Case where object instantly snaps into hand position
        {
            heldObject.transform.position = transform.position + (heldObject.transform.position - heldObject.grabOrientation.position);
        }
        if (heldObject.forceGrabRotation && grabSnapStrength == 1) //Case where object instantly snaps into hand rotation
        {
            //NOT FINISHED
        }
    }
    private void Release()
    {
        //Function: Releases currently-held grabbable

        //Cleanup:
        heldObject.transform.parent = transform.root; //Unchild held object
        heldObject.IsReleased();                      //Indicate that object has been released
        heldObject = null;                            //Indicate object is no longer being held
    }
}
