using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : SipNode
{
    [SerializeField]
    private List<Animator> toAnimate;
    [SerializeField]
    private List<GameObject> toDisable;
    [SerializeField]
    private List<GameObject> toEnable;
    private static bool gameStarted;
    private void Start()
    {
        gameStarted = false;
    }
    public override void TakeSip()
    {
        if (gameStarted) return;
        base.TakeSip();
        foreach(Animator i in toAnimate)
        {
            if (i != null)
                i.Play("OnStartGame");
        }
        foreach(GameObject i in toDisable)
        {
            if (i != null)
                i.SetActive(false);
        }
        foreach(GameObject i in toEnable)
        {
            if (i != null)
                i.SetActive(true);
        }
        gameStarted = true;
        ImportantObject.NoImportantObjects = true;
    }
}
