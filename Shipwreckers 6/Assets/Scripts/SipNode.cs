using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SipNode : MonoBehaviour
{
    [SerializeField]
    private Transform refPoint;

    private AudioSource aud;
    private float sipWait = 0.0f;

    private void Awake()
    {
        aud = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (sipWait > 0) sipWait -= Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (sipWait > 0) return;
        if (other.gameObject.layer == 7)
        {
            if (transform.position.y < refPoint.position.y)
            {
                TakeSip();
            }
        }
    }

    public virtual void TakeSip()
    {
        sipWait = 1.0f;
        aud.Play();
    }
}
