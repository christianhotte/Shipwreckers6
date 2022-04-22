using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonDebugTarget : MonoBehaviour
{
    IEnumerator ShootOccasionally()
    {
        for (;;)
        {
            ShipCannon.FireAllCannonsAtTarget(transform, 90, 2f);
            yield return new WaitForSeconds(4f);
        }
    }
    private void Start()
    {
        StartCoroutine(ShootOccasionally());
    }
}
