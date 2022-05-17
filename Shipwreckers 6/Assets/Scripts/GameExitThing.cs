using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameExitThing : MonoBehaviour
{
    public void ExitTheGame(InputAction.CallbackContext context)
    {
        Debug.Log("GAME EXIT");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
