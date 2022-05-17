using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateSipNode : SipNode
{
    public static bool activated = false;
    public override void TakeSip()
    {
        base.TakeSip();
        activated = true;
    }
}
