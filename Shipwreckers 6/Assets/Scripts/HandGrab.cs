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
    [SerializeField] [Tooltip("Determines how many previous velocities to remember for averaging throw velocity")]                 private int velMemoryLength;

    //Runtime Memory Vars:
    private List<Vector3> velocityMem = new List<Vector3>();   //List of raw velocity vectors from last few physics updates (in order from latest to oldest)
    private List<Vector3> angularVelMem = new List<Vector3>(); //List of raw angular velocity vectors from last few physics updates (in order from latest to oldest)
    private Vector3 prevPosition;                              //Last position hand object was in, used to compute momentary velocity
    private Quaternion prevRotation;                           //Last rotation hand object had, used to compute momentary angular velocity

    //RUNTIME METHODS:
    private void Start()
    {
        //Initialize variables:
        prevPosition = transform.position; //Get current position of hand to start off
        prevRotation = transform.rotation; //Get current rotation of hand to start off
        velocityMem.Add(Vector3.zero);     //Set first velocity in memory to 0 (array needs at least one entry for safety)
        angularVelMem.Add(Vector3.zero);   //Set first angular velocity im memory to 0 (array needs at least one entry for safety)
    }
    private void FixedUpdate()
    {
        //Update current velocity:
        Vector3 currentVelocity = (transform.position - prevPosition) / Time.fixedDeltaTime; //Get velocity this physics update
        prevPosition = transform.position;                                                   //Update prevPosition once used
        velocityMem.Insert(0, currentVelocity);                                              //Insert latest velocity at beginning of memory list
        if (velocityMem.Count > velMemoryLength) velocityMem.RemoveAt(velMemoryLength);      //Remove oldest item in memory if list is overfull

        //Update current angular velocity:
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(prevRotation);        //Get quaternion representing rotation made last update
        Vector3 eulerRot = new Vector3( Mathf.DeltaAngle( 0, deltaRot.eulerAngles.x ),      //Get angle difference for X axis
                                        Mathf.DeltaAngle( 0, deltaRot.eulerAngles.y ),      //Get angle difference for Y axis
                                        Mathf.DeltaAngle( 0, deltaRot.eulerAngles.z ));     //Get angle difference for Z axis
        Vector3 currentAngVel = (eulerRot / Time.fixedDeltaTime) * Mathf.Deg2Rad;           //Get angles from degrees per fixedupdate to radians per second
        prevRotation = transform.rotation;                                                  //Update previous rotation once used
        angularVelMem.Insert(0, currentAngVel);                                             //Insert latest angular velocity at beginning of memory list
        if (angularVelMem.Count > velMemoryLength) angularVelMem.RemoveAt(velMemoryLength); //Remove oldest item in memory if list is overfull

        //Update held object orientation:
        if (heldObject != null) //Player is currently holding an object
        {
            if (heldObject.forceGrabPosition && heldObject.transform.localPosition != -heldObject.grabOrientation.localPosition) //Object needs to be lerped into position
            {
                //Lerp object toward desired position:
                Vector3 targetPosition = transform.position + (heldObject.transform.position - heldObject.grabOrientation.position); //Get target transform position
                Vector3 newPosition = Vector3.Lerp(heldObject.transform.position, targetPosition, grabSnapStrength);                 //Get new position by lerping with snap strength
                if (Vector3.Distance(targetPosition, newPosition) < 0.01) newPosition = targetPosition;                              //Just snap to target if close enough
                heldObject.transform.position = newPosition;                                                                         //Apply new position to object
            }
            if (heldObject.forceGrabRotation && heldObject.transform.rotation != transform.rotation) //Object neewds to be lerped into rotation
            {
                //Lerp object toward desired rotation:
                Quaternion targetRotation = heldObject.grabOrientation.localRotation;                                            //Get target local rotation
                Quaternion newRotation = Quaternion.Slerp(heldObject.transform.localRotation, targetRotation, grabSnapStrength); //Get new rotation by lerping with snap strength
                if (Quaternion.Angle(targetRotation, newRotation) < 0.01) newRotation = targetRotation;                          //Just snap to target if close enough
                heldObject.transform.localRotation = newRotation;                                                                //Apply new rotation to object
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
        heldObject.transform.parent = transform; //Make object child of the hand
        if (heldObject.forceGrabPosition && grabSnapStrength == 1) //Case where object instantly snaps into hand position
        {
            heldObject.transform.position = transform.position + (heldObject.transform.position - heldObject.grabOrientation.position);
        }
        if (heldObject.forceGrabRotation && grabSnapStrength == 1) //Case where object instantly snaps into hand rotation
        {
            heldObject.transform.localRotation = heldObject.grabOrientation.localRotation; //Just match target rotation as set by grabOrientation transform
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
        heldObject.transform.parent = transform.root;              //Unchild held object
        heldObject.IsReleased(this);                               //Indicate that object has been released
        heldObject.rb.velocity = SmoothedCurrentVelocity();        //Send current velocity (smoothed)
        heldObject.rb.angularVelocity = SmoothedAngularVelocity(); //Send current angular velocity (smoothed)
        heldObject = null;                                         //Indicate object is no longer being held
    }

    //UTILITY METHODS:
    /// <summary>
    /// Returns average of all stored velocities in recent memory (smoothed velocity).
    /// </summary>
    private Vector3 SmoothedCurrentVelocity()
    {
        Vector3 totalVelocity = Vector3.zero;                      //Initialize container for storing sum of velocity memory list
        foreach (Vector3 vel in velocityMem) totalVelocity += vel; //Add each velocity in memory to total velocity vector
        return totalVelocity / velocityMem.Count;                  //Return average velocity between all velocities in memory
    }
    /// <summary>
    /// Returns average of all stored angular velocities in recent memory (smoothed angular velocity).
    /// </summary>
    private Vector3 SmoothedAngularVelocity()
    {
        Vector3 totalAngVel = Vector3.zero;                        //Initialize container for storing sum of angular velocity memory list
        foreach (Vector3 vel in angularVelMem) totalAngVel += vel; //Add each angular velocity in memory (should already be in radians) to total velocity vector
        return totalAngVel / angularVelMem.Count;                  //Return average angular velocity between all angular velocities in memory
    }
}
