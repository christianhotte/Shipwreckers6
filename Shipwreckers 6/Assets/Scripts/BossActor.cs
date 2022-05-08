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

[System.Serializable]
public struct BossPhase
{
    public string phaseName;
    public int health;
    public ShipActionSet[] actionTable;
    public string startAnimation;
    public float startAnimationDuration;
    public int nextPhaseIndex;
}

public class BossActor : MonoBehaviour, IShootable
{
    [SerializeField]
    private Transform playerHead;
    private PolarMover pmove;
    private Animator anim;
    [SerializeField]
    private BossPhase[] phases;
    private ShipActionSet[] randomActionTable;
    [SerializeField]
    private Transform mobileorigin;
    [SerializeField]
    private MeshRenderer[] meshList;
    private List<Material> colorList;
    private int currentSequence = 0;
    private int currentActionInSequence = 0;

    private bool dead;
    private int phaseIndex = 0;
    private bool phaseTransforming = false;

    [SerializeField]
    private GameObject exploPrefab;

    private AudioSource audGen; // General audio player
    [SerializeField]
    private AudioSource creakAud; // Creak audio source
    [SerializeField]
    private AudioSource splashAud; // Splash audio source
    [SerializeField]
    private AudioSource takeDmgAud; // Take damage audio source

    [SerializeField]
    private AudioSource smallMusic; // Player for small theme
    [SerializeField]
    private AudioSource largeMusic; // Player for large theme

    private void Awake()
    {
        pmove = GetComponent<PolarMover>();
        anim = GetComponent<Animator>();
        StartCoroutine(NewPhase(0));
        colorList = new List<Material>();
        foreach(MeshRenderer mr in meshList)
        {
            if (mr != null)
            {
                foreach (Material mat in mr.materials)
                {
                    colorList.Add(mat);
                }
            }
        }
        audGen = GetComponent<AudioSource>();
    }
    private void Update()
    {
        mobileorigin.transform.localScale = Vector3.Lerp(mobileorigin.transform.localScale, Vector3.one, Time.deltaTime);
        foreach (Material toColor in colorList)
        {
            toColor.color = Color.Lerp(toColor.color, Color.white, Time.deltaTime*2.0f);
        }
    }

    private void ShuffleActionTable()
    {
        randomActionTable = phases[phaseIndex].actionTable.OrderBy(x => (int)Random.Range(0,9999999)).ToArray();
    }

    IEnumerator NewPhase(int newPhaseIndex)
    {
        phaseIndex = newPhaseIndex;
        if (phaseIndex < 0 || phaseIndex >= phases.Length)
        {
            dead = true;
            Destroy(gameObject, 10);
        }
        else
        {
            phaseTransforming = true;
            anim.Play(phases[phaseIndex].startAnimation);
            yield return new WaitForSeconds(phases[phaseIndex].startAnimationDuration);
            phaseTransforming = false;
            StartCoroutine(ShipActionLoop());
        }
    }

    IEnumerator ShipActionLoop()
    {
        while (!dead && !phaseTransforming)
        {
            ShuffleActionTable();
            currentSequence = 0;
            if (randomActionTable.Length < 1) break;
            while (currentSequence < randomActionTable.Length)
            {
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

    public void Shoot(CannonAmmoConfig cac, Vector3 hitp)
    {
        if (phaseTransforming || dead)
        {
            return;
        }
        print("Ship's been shot");
        phases[phaseIndex].health -= 1;
        if (phases[phaseIndex].health < 1)
        {
            // Shot and phase ends
            StopAllCoroutines();
            StartCoroutine(NewPhase(phases[phaseIndex].nextPhaseIndex));
        }
        else
        {
            // Shot and phase continues
            mobileorigin.localScale = new Vector3(1.2f, 1.2f, 1.0f);
            foreach (Material toColor in colorList)
            {
                toColor.color = Color.red;
            }
        }
        // Spawn explosion
        GameObject exp = Instantiate(exploPrefab);
        exp.transform.position = hitp;
        Destroy(exp, 5.0f);
    }

    //----------------------------------------
    // --- SOUND EFFECT METHODS ---
    //----------------------------------------
    public void PlayCreak()
    {
        if (creakAud != null) creakAud.Play();
    }
    public void PlaySplash()
    {
        if (splashAud != null) splashAud.Play();
    }
    public void PlayTakeDmg()
    {
        if (takeDmgAud != null) takeDmgAud.Play();
    }

    //----------------------------------------
    // --- SOUND EFFECT METHODS ---
    //----------------------------------------
    public void StartSmallMusic()
    {
        smallMusic.Play();
    }
    public void StopSmallMusic()
    {
        smallMusic.Stop();
    }
    public void StartLargeMusic()
    {
        largeMusic.Play();
    }
    public void StopLargeMusic()
    {
        largeMusic.Stop();
    }

    //----------------------------------------
    // --- FIRE METHODS ---
    //----------------------------------------
    public void FireCannon()
    {
        //ShipCannon.FireAllCannonsAtTarget(playerHead, 90.0f, 0.1f);
        ShipCannon.FireRandomCannon(playerHead, 90.0f);
    }
    public void FireBomb()
    {
        //ShipCannon.FireAllCannonsAtTarget(playerHead, 90.0f, 0.1f);
        ShipCannon.FireRandomBomb(playerHead, 90.0f);
    }
    public void FireSmall()
    {
        //ShipCannon.FireAllCannonsAtTarget(playerHead, 90.0f, 0.1f);
        ShipCannon.FireSmall(playerHead, 90.0f);
    }

    //----------------------------------------
    // --- EFFECT METHODS ---
    //----------------------------------------
    public void ExplodeMast()
    {
        MastExploder.ExplodeNextMast();
    }
}
