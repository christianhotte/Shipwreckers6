using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Deformation;

public class SwordDriver : MonoBehaviour
{
    //Objects & Components:
    private Holsterable holsterable; //This sword's holsterable component
    private AudioSource audioSource; //This sword's audio source component

    //Settings:
    [Header("Bifurcation Settings:")]
    [SerializeField] [Tooltip("Sound made when sword cuts an object")] private AudioClip cutSound;
    [SerializeField] [Tooltip("How much force is applied to object halves when an object is cut")] private float cutSeparationForce;

    //Runtime Vars:


    //RUNTIME METHODS:
    private void Start()
    {
        //Get objects and components:
        holsterable = GetComponent<Holsterable>(); //Get holsterable component
        audioSource = GetComponent<AudioSource>(); //Get audio source component
    }

    //FUNCTIONALITY METHODS:
    public void CutProcedure(ShipCannonProjectile projectile)
    {
        //Validity checks:
        if (holsterable.isHolstered) return; //Ignore if sword is holstered

        //Determine cut direction:
        Vector3 cutDirection = GetComponent<Rigidbody>().velocity; //TEMP

        //Cut projectile:
        if (projectile.AttemptToCut(cutDirection, cutSeparationForce)) //Projectile can be successfully cut in twain
        {
            audioSource.PlayOneShot(cutSound); //Play cut sound on successful cut
        }
    }
}
