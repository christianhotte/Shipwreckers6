using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitBottle : SipNode
{
    public override void TakeSip()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
