using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImportantObject : MonoBehaviour
{
    [SerializeField]
    private float maxDistFromStart;
    [SerializeField]
    private float minTimoutDistFromStart;
    [SerializeField]
    private int timeoutFrames;
    private int currentTimeoutFrames;
    private Vector3 startPos;
    private Rigidbody rb;
    private Grabbable grabscript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabscript = GetComponent<Grabbable>();
    }
    private void Start()
    {
        startPos = transform.position;
    }

    private void BackToStart()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        transform.position = startPos;
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(startPos, transform.position) > maxDistFromStart)
        {
            BackToStart();
        }
        else if (Vector3.Distance(startPos, transform.position) > minTimoutDistFromStart && !grabscript.beingGrabbed)
        {
            if (currentTimeoutFrames > 0) currentTimeoutFrames -= 1;
            else BackToStart();
        }
        else
        {
            currentTimeoutFrames = timeoutFrames;
        }
    }
}
