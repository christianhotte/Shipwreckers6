using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoiscenseForGettingStabbed : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        print("oog");
        if (collider.gameObject.layer == 14) PlayerHealthManager.HurtPlayer(1);
    }
}
