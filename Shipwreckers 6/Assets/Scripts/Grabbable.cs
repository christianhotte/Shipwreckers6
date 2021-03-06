using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attached to an gameobject which can be grabbed by player, contains information related to grabbing.
/// </summary>
public class Grabbable : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    internal Rigidbody rb;              //Grabbable rigidbody component
    internal Collider coll;             //Grabbable collider component
    internal Transform grabOrientation; //Transform used to orient object when grabbed (optional)
    internal HandGrab currentHand;      //Hand currently holding this object
    internal bool beingGrabbed = false;
    internal bool hasBeenGrabbed = false;
    private ImportantObject importantObj;

    //Settings:
    [Tooltip("Causes object to snap to specific position when grabbed")] public bool forceGrabPosition;
    [Tooltip("Causes object to snap to specific rotation when grabbed")] public bool forceGrabRotation;
    [Tooltip("Whether or not object may currently be grabbed")]          public bool isGrabbable = true;

    //Runtime Memory Vars:
    public UnityAction OnGrab;

    //RUNTIME METHODS:
    public virtual void Awake()
    {
        //Get objects & components:
        if (!TryGetComponent(out rb)) { Debug.LogError(name + " needs a rigidbody to be grabbable"); Destroy(this); }  //Make sure object has a rigidbody
        if (!TryGetComponent(out coll)) { Debug.LogError(name + " needs a collider to be grabbable"); Destroy(this); } //Make sure object has a collider
        importantObj = GetComponent<ImportantObject>();
        if (forceGrabPosition || forceGrabRotation) //Object needs an orientation transform
        {
            grabOrientation = transform.Find("grabOrientation"); //Get orientation transform
            if (grabOrientation == null) { Debug.LogError(name + " needs a child named 'grabOrientation' in order to force grab position or rotation"); Destroy(this); } //Make sure object has orientation transform
        }

        //Subscriptions:
        OnGrab += OnGrabEvent;
    }
    private void OnDisable()
    {
        //Unsubscriptions:
        OnGrab -= OnGrabEvent;
    }

    //PUBLIC METHODS:
    /// <summary>
    /// Called when this object is grabbed by player.
    /// </summary>
    public virtual void IsGrabbed(HandGrab controller)
    {
        rb.isKinematic = true;    //Make object kinematic (to negate gravity)
        currentHand = controller; //Store hand grabbing this object
        beingGrabbed = true;
        hasBeenGrabbed = true;
        foreach (Animator anim in Fingerer.main.anims) anim.SetInteger("GrabType", 2);
        OnGrab();
    }
    /// <summary>
    /// Called when this object is released by player.
    /// </summary>
    public virtual void IsReleased(HandGrab controller)
    {
        rb.isKinematic = false; //Re-enable dynamic object movement
        foreach (Animator anim in Fingerer.main.anims) anim.SetInteger("GrabType", 0);
        currentHand = null;     //Remove reference to hand controller
        beingGrabbed = false;
    }
    private void OnGrabEvent() { }
}
