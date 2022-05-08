using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootableRerouter : MonoBehaviour, IShootable
{
    [SerializeField] private GameObject target;
    private IShootable shootable;

    private void Awake()
    {
        if (target == null) { Debug.LogWarning("Shootable rerouter has no target and is being destroyed for testing purposes"); Destroy(this); return; }
        if (!target.TryGetComponent(out shootable)) { Debug.LogError("Target does not fungible"); Destroy(this); }
    }

    public void Shoot(CannonAmmoConfig cac, Vector3 hitp) {
        Debug.Log("Rerouter hit!");
        shootable.Shoot(cac, hitp); 
    }
}
