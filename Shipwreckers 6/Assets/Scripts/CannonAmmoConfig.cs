using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data used by CannonAmmo to determine projectile properties (intended to make the process of making an object CannonAmmo easier).
/// </summary>
[CreateAssetMenu(fileName = "AmmoConfig_Default", menuName = "ScriptableObjects/CannonAmmoConfig", order = 1)]
public class CannonAmmoConfig : ScriptableObject
{
    [Header("Gameplay Settings:")]
    [Tooltip("How much damage this projectile deals upon a direct hit")] public int rawDamage;
}
