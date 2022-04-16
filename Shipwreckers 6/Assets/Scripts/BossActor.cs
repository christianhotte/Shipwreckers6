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
    // Time waited before another action is taken
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
    private PolarMover pmove;
    private Animator anim;
    [SerializeField]
    private BossActionSequence[] actionTable;
    private BossActionSequence[] randomActionTable;
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
            currentActionInSequence = 0;
            while (currentActionInSequence < randomActionTable.Length)
            {
                Debug.Log(randomActionTable[currentActionInSequence].sequenceName);
                currentActionInSequence++;
                yield return new WaitForSeconds(1.0f);
            }
            Debug.Log("--------------");
        }
    }

    public void Shoot(CannonAmmoConfig cac)
    {
        throw new System.NotImplementedException();
    }
}
