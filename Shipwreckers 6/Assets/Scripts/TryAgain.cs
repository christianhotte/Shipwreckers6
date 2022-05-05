using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TryAgain : SipNode
{
    public override void TakeSip()
    {
        SceneManager.LoadScene("finscene");
    }
}
