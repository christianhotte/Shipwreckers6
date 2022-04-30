using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct BossAction
{
    // Animation name to be played
    public string animName;
    // Radians to be rotated
    public float rotation_min;
    public float rotation_max;
    // Units to move to/from player
    public float approach_min;
    public float approach_max;
    // If checked, the movement will result in an instant teleport
    public bool does_teleport;
    // If checked, the 'rotation' and 'approach' coords will be absolute instead of relative
    public bool move_absolute;
    // Time waited before animation plays
    public float animation_wait_time;
    // Time waited before movement is made
    public float movement_wait_time;
    // Time waited before another action is taken (basically how long does this action take in total)
    public float next_action_wait_time;
}

[System.Serializable]
public struct BossActionSequence
{
    // Set of boss actions to be played in order
    public BossAction[] set;
    // This is the name of the sequence, it's more for debug than anything else.
    public string sequenceName;
}

public class BossActor : MonoBehaviour, IShootable
{
    [SerializeField]
    private Transform playerHead;
    private PolarMover pmove;
    private Animator anim;
    [SerializeField]
    private ShipActionSet[] actionTable;
    private ShipActionSet[] randomActionTable;
    /*
    private BossActionSequence[] actionTable;
    private BossActionSequence[] randomActionTable;
    */
    private int currentSequence = 0;
    private int currentActionInSequence = 0;

    [SerializeField]
    private int health;

    private void Awake()
    {
        pmove = GetComponent<PolarMover>();
        anim = GetComponent<Animator>();
        ShuffleActionTable();
        StartCoroutine(ShipActionLoop());
    }

    private void ShuffleActionTable()
    {
        randomActionTable = actionTable.OrderBy(x => (int)Random.Range(0,9999999)).ToArray();
    }

    IEnumerator ShipActionLoop()
    {
        while (health > 0)
        {
            ShuffleActionTable();
            currentSequence = 0;
            if (randomActionTable.Length < 1) break;
            while (currentSequence < randomActionTable.Length)
            {
                Debug.Log(randomActionTable[currentSequence].bas.sequenceName);
                currentActionInSequence = 0;
                while (currentActionInSequence < randomActionTable[currentSequence].bas.set.Length)
                {
                    StartCoroutine(WaitThenMove(randomActionTable[currentSequence].bas.set[currentActionInSequence]));
                    StartCoroutine(WaitThenAnimate(randomActionTable[currentSequence].bas.set[currentActionInSequence]));
                    yield return new WaitForSeconds(randomActionTable[currentSequence].bas.set[currentActionInSequence].next_action_wait_time);
                    currentActionInSequence++;
                }
                currentSequence++;
            }
            Debug.Log("--------------");
        }
        yield return new WaitForSeconds(1.0f);
    }
    IEnumerator WaitThenMove(BossAction action)
    {
        yield return new WaitForSeconds(action.movement_wait_time);
        float finalAngle = Random.Range(action.rotation_min, action.rotation_max);
        float finalApproach = Random.Range(action.approach_min, action.approach_max);
        if (action.move_absolute)
            pmove.PolarWaypointSetAbsolute(finalAngle, finalApproach);
        else
            pmove.PolarWaypointSetRelative(finalAngle, finalApproach);
        if (action.does_teleport) pmove.SnapToWaypoint();
    }
    IEnumerator WaitThenAnimate(BossAction action)
    {
        yield return new WaitForSeconds(action.animation_wait_time);
        if (anim != null)
        {
            anim.Play(action.animName);
        }
    }

    public void FireCannon()
    {
        ShipCannon.FireAllCannonsAtTarget(playerHead, 25.0f, 0.1f);
    }

    public void Shoot(CannonAmmoConfig cac)
    {
        print("Ship's been shot");
    }
}
