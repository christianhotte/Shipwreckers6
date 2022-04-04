using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BossAction
{
    // Animation name to be played
    string animName;
    // Radians to be rotated
    float rotation_min;
    float rotation_max;
    // Units to move to/from player
    float approach_min;
    float approach_max;
    // If checked, the movement will result in an instant teleport
    float does_teleport;
    // If checked, the 'rotation' and 'approach' coords will be absolute instead of relative
    float move_absolute;
    // Time waited before animation plays
    float animation_wait_time;
    // Time waited before movement is made
    float movement_wait_time;
    // Time waited before another action is taken
    float next_action_wait_time;
}

public class BossActor : MonoBehaviour, IShootable
{
    private PolarMover pmove;
    private Animator anim;

    [SerializeField]
    private BossAction[][] actionTable;

    private void Awake()
    {
        pmove = GetComponent<PolarMover>();
    }

    private void Update()
    {
        
    }

    private void PerformAction(BossAction action)
    {

    }

    public void Shoot(CannonAmmoConfig cac)
    {
        throw new System.NotImplementedException();
    }
}
