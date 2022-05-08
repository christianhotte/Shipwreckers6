using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallSpawner : MonoBehaviour
{
    //Objects & Components:
    [SerializeField, Tooltip("")] private GameObject ammoPrefab;
    [SerializeField, Tooltip("")] private Material badMaterial;
    internal AudioSource audioSource;

    //Settings:
    [Tooltip("")] public List<AudioClip> grabSounds = new List<AudioClip>();

    //Runtime Vars:
    internal GameObject currentAmmo;

    //RUNTIME METHODS:
    public virtual void Awake()
    {
        //Get objects & components:
        if (!TryGetComponent(out audioSource)) { Debug.LogWarning("Cannonballspawner has no audio source"); }

        //Initialize:
        GenerateNewAmmo();
    }
    private void OnDisable()
    {
        if (currentAmmo != null) { currentAmmo.GetComponent<Grabbable>().OnGrab -= OnAmmoGrabbed; }
    }
    public virtual void OnAmmoGrabbed()
    {
        currentAmmo.GetComponent<Grabbable>().OnGrab -= OnAmmoGrabbed;
        currentAmmo.GetComponent<Renderer>().enabled = true;
        GenerateNewAmmo();
        audioSource.PlayOneShot(grabSounds[Random.Range(0, grabSounds.Count)]);
    }

    //UTILITY METHODS:
    public void GenerateNewAmmo()
    {
        currentAmmo = Instantiate(ammoPrefab, transform);
        if (badMaterial != null) currentAmmo.GetComponent<Renderer>().material = badMaterial;
        currentAmmo.GetComponent<Renderer>().enabled = false;
        currentAmmo.transform.position = transform.position;
        currentAmmo.GetComponent<Rigidbody>().isKinematic = true;
        currentAmmo.GetComponent<Grabbable>().OnGrab += OnAmmoGrabbed;
    }
}
