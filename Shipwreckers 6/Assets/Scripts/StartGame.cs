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
    public override void TakeSip()
    {
        if (gameStarted) return;
        base.TakeSip();
        foreach(Animator i in toAnimate)
        {
            i.Play("OnStartGame");
        }
        foreach(GameObject i in toDisable)
        {
            i.SetActive(false);
        }
        foreach(GameObject i in toEnable)
        {
            i.SetActive(true);
        }
        gameStarted = true;
        ImportantObject.NoImportantObjects = true;
    }
}