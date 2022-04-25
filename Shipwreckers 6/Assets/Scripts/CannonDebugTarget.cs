using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonDebugTarget : MonoBehaviour
{
    IEnumerator ShootOccasionally()
    {
        for (;;)
        {
            ShipCannon.FireAllCannonsAtTarget(transform, 90, 20f);
            yield return new WaitForSeconds(20f);
        }
    }
    private void Start()
    {
        StartCoroutine(ShootOccasionally());
    }
}
