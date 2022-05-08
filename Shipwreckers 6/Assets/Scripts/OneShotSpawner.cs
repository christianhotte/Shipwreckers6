using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotSpawner : CannonBallSpawner
{
    public override void Awake()
    {
        if (!TryGetComponent(out audioSource)) { Debug.LogWarning("Cannonballspawner has no audio source"); }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11 && HandGrab.main.holdingGrab && HandGrab.main.heldObject == null)
        {
            GenerateNewAmmo();
            currentAmmo.transform.localScale = Vector3.one * 0.7f;
            HandGrab.main.hoverObject = currentAmmo.GetComponent<Grabbable>();
            HandGrab.main.Grab();
            HandGrab.main.GetComponent<AudioSource>().PlayOneShot(grabSounds[Random.Range(0, grabSounds.Count)]);
            Destroy(transform.parent.gameObject);
        }
    }

}