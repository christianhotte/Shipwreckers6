using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to an gameobject which can be grabbed by player, contains information related to grabbing.
/// </summary>
public class Grabbable : MonoBehaviour
{
    //Classes, Enums & Structs:

    //Objects & Components:
    internal Rigidbody rb;              //Grabbable rigidbody component
    internal Transform grabOrientation; //Transform used to orient object when grabbed (optional)

    //Settings:
    [Tooltip("Causes object to snap to specific position when grabbed")] public bool forceGrabPosition;
    [Tooltip("Causes object to snap to specific rotation when grabbed")] public bool forceGrabRotation;

    //Runtime Memory Vars:


    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        if (!TryGetComponent(out rb)) { Debug.LogError(name + " needs a rigidbody to be grabbable"); Destroy(this); }               //Make sure object has a rigidbody
        if (!TryGetComponent(out Collider collider)) { Debug.LogError(name + " needs a collider to be grabbable"); Destroy(this); } //Make sure object has a collider
        if (forceGrabPosition || forceGrabRotation) //Object needs an orientation transform
        {
            grabOrientation = transform.Find("grabOrientation"); //Get orientation transform
            if (grabOrientation == null) { Debug.LogError(name + " needs a child named 'grabOrientation' in order to force grab position or rotation"); Destroy(this); } //Make sure object has orientation transform
        }
    }

    //PUBLIC METHODS:
    /// <summary>
    /// Call when this object is grabbed by player.
    /// </summary>
    public void IsGrabbed()
    {
        rb.isKinematic = true; //Make object kinematic (to negate gravity)
    }
    /// <summary>
    /// Call when this object is released by player.
    /// </summary>
    public void IsReleased()
    {
        rb.isKinematic = false; //Re-enable dynamic object movement
    }
}
