using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyOrienter : MonoBehaviour
{
    public static BodyOrienter main;

    private void Awake()
    {
        main = this;
    }

    private void Update()
    {
        Vector3 flattenedForward = transform.parent.forward; //Get parent transform's orientation
        flattenedForward.y = 0;      //Flatten axes
        flattenedForward = flattenedForward.normalized;      //Re-normalize vector now that it has been flattened
        transform.rotation = Quaternion.LookRotation(flattenedForward, Vector3.up);
    }
}
