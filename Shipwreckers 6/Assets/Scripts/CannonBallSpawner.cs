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
    [Tooltip("List of sounds to randomly pull from when object is being grabbed by player")] public List<AudioClip> grabSounds = new List<AudioClip>();
    [SerializeField, Range(0, 1), Tooltip("Haptic impulse amplitude for grab effect")]       private float grabVibeAmp;
    [SerializeField, Tooltip("Haptic impulse duration for grab effect (in seconds)")]        private float grabVibeDuration;

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
        //Release ammo:
        currentAmmo.GetComponent<Grabbable>().OnGrab -= OnAmmoGrabbed; //Unsubscribe from ammo-specific grab event
        currentAmmo.GetComponent<Renderer>().enabled = true;           //Make ammo visible

        //Cleanup:
        GenerateNewAmmo(); //Spawn new ammo item

        //Effects:
        audioSource.PlayOneShot(grabSounds[Random.Range(0, grabSounds.Count)]); //Play grab sound effect
        HandGrab.main.SendHapticImpulse(grabVibeAmp, grabVibeDuration);         //Send vibration to player hand
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
