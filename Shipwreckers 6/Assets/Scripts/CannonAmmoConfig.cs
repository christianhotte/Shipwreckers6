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
    [Tooltip("How much damage this projectile deals upon a direct hit")]               public int rawDamage;
    [Tooltip("If true, this object cannot be fit into cannon with other projectiles")] public bool isLarge;
    [Tooltip("How long after being dropped cannonballs take to despawn")] public float unheldDespawnTime;
    public float maxFlightTime;
}
