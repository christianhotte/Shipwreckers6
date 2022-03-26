using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopsInSand : Grabbable
{
    private Vector3 lastSeenVelocity;
    private void FixedUpdate()
    {
        if (rb.isKinematic) return;
        if (rb.velocity.magnitude > 0.0f)
        {
            lastSeenVelocity = rb.velocity;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (rb.isKinematic) return;
        //Basically if this object falls into sand
        if (other.gameObject.layer == 6)
        {
            transform.position += new Vector3(
                    Mathf.Clamp(lastSeenVelocity.x, -7, 7),
                    Mathf.Clamp(lastSeenVelocity.y, -7, 7),
                    Mathf.Clamp(lastSeenVelocity.z, -7, 7)
                )/55.0f;
            Debug.Log(lastSeenVelocity);
            rb.isKinematic = true;
        }
    }

}
