using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deformable : MonoBehaviour
{
    //Description: Allows an object with an attached mesh and mesh-based physics to be modified by deforming systems

    //Objects & Components:
    internal MeshFilter meshFilter;     //Attached MeshFilter component
    internal MeshRenderer meshRenderer; //Attached MeshRenderer component
    internal MeshCollider meshCollider; //Attached MeshCollider component
    internal Rigidbody rb;              //Attached RigidBody component

    //RUNTIME METHODS:
    private void Awake()
    {
        //Validity checks:
        if (!gameObject.TryGetComponent(out meshFilter) ||   //Get mesh filter if possible
            !gameObject.TryGetComponent(out meshRenderer) || //Get mesh renderer if possible
            !gameObject.TryGetComponent(out meshCollider) || //Get mesh collider if possible
            !gameObject.TryGetComponent(out rb))             //Get rigidbody if possible
        {
            //Missing component protocol:
            Debug.LogError(gameObject.name + " is missing a component needed by Deformable"); //Post error
            Destroy(this); //Destroy this script to prevent further errors
        }
    }
}
