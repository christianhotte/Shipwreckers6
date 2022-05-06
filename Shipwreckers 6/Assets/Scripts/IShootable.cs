using UnityEngine;

public interface IShootable
{
    void Shoot(CannonAmmoConfig cac, Vector3 hitPoint);
}
