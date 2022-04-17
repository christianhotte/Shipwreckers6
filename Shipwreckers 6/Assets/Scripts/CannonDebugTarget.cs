using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonDebugTarget : MonoBehaviour
{
    IEnumerator ShootOccasionally()
    {
        for (;;)
        {
            ShipCannon.FireAllCannonsAtTarget(transform, 90, 0.7f);
            yield return new WaitForSeconds(3f);
        }
    }
    private void Start()
    {
        StartCoroutine(ShootOccasionally());
    }
}
