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
    private Quaternion startRot;
    private Rigidbody rb;
    private Grabbable grabscript;
    private CannonAmmo ammoscript;
    public static bool NoImportantObjects = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabscript = GetComponent<Grabbable>();
        ammoscript = GetComponent<CannonAmmo>();
    }
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void BackToStart()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        transform.position = startPos;
        transform.rotation = startRot;
        transform.parent = transform.root;
        if (grabscript != null) grabscript.isGrabbable = true;
        Transform aura = transform.Find("Aura");
        if (aura != null)
        {
            Destroy(aura.gameObject);
        }
    }
    public bool IsLoaded()
    {
        if (ammoscript == null) return false;
        return ammoscript.isLoaded;
    }
    private bool IsBeingGrabbed()
    {
        if (grabscript == null) return false;
        return grabscript.beingGrabbed;
    }

    private void FixedUpdate()
    {
        if (IsBeingGrabbed()) return;
        if (IsLoaded()) return;
        if (NoImportantObjects) return;
        if (Vector3.Distance(startPos, transform.position) > maxDistFromStart)
        {
            BackToStart();
        }
        else if (Vector3.Distance(startPos, transform.position) > minTimoutDistFromStart)
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
