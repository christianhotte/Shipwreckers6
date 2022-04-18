using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sequence", menuName = "ScriptableObjects/ShipActionSet", order = 1)]
public class ShipActionSet : ScriptableObject
{
    public BossActionSequence bas;
}
