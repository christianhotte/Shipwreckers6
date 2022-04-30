using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Facilitates loading of player's main cannon.
/// </summary>
public class CannonLoader : MonoBehaviour
{
    //Objects & Components:
    private Cannon cannonController; //Cannon controller script in parent

    //RUNTIME METHODS:
    private void Awake()
    {
        //Get objects & components:
        if (!transform.parent.TryGetComponent(out cannonController)) { Debug.LogError("CannonLoader could not find cannon controller script in parent"); Destroy(this); }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CannonAmmo ammoController) && ammoController.grabbable.hasBeenGrabbed) cannonController.Load(ammoController); //Load object into cannon if it is a cannonball
    }
}
