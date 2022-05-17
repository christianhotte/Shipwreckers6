using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReCenterer : MonoBehaviour
{
    public void OnReCenterInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Vector3 targetPos = transform.GetChild(0).localPosition;
        Vector3 newPosition = new Vector3(-targetPos.x, 0, -targetPos.z);
        transform.localPosition = newPosition;
    }
}
