using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonDebugTarget : MonoBehaviour
{
    IEnumerator ShootOccasionally()
    {
        for (;;)
        {
            foreach (ShipCannon cannon in ShipCannon.allCannons)
            {
                cannon.FireAtTarget(transform);
                print("Cannonfired");
            }
            yield return new WaitForSeconds(3f);
        }
    }
    private void Start()
    {
        StartCoroutine(ShootOccasionally());
    }
}
