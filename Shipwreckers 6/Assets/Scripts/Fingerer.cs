using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fingerer : MonoBehaviour
{
    public static Fingerer main;
    public GameObject spurtPrefab;
    public List<GameObject> hands = new List<GameObject>();
    internal List<Animator> anims = new List<Animator>();

    public bool debugDestroyFinger;

    private void Awake()
    {
        main = this;
    }
    private void Start()
    {
        foreach (GameObject hand in hands)
        {
            anims.Add(hand.GetComponent<Animator>());
        }
    }
    private void Update()
    {
        if (debugDestroyFinger)
        {
            DestroyFinger();
            debugDestroyFinger = false;
        }
    }
    public void DestroyFinger()
    {
        if (hands.Count <= 1) { Debug.LogWarning("No fingers left to destroy"); return; }
        
        GameObject outgoingHand = hands[0];
        GameObject spurt = Instantiate(spurtPrefab, transform);
        spurt.transform.position = outgoingHand.transform.Find("SpurtPoint").position;
        hands.RemoveAt(0);
        anims.RemoveAt(0);
        Destroy(outgoingHand);

        if (hands.Count == 0) { return; }

        GameObject incomingHand = hands[0];
        incomingHand.GetComponentInChildren<Renderer>().enabled = true;
    }
}
