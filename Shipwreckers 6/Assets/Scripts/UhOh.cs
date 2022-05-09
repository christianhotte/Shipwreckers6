using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UhOh : MonoBehaviour
{
    bool triggered;
    public AudioClip theSound;
    public float theAngle;

    private void Update()
    {
        Vector3 viewport = Camera.main.WorldToViewportPoint(transform.position);
        bool inFrontOfCamera = viewport.z > 0;
        float dist = Vector3.Angle(Camera.main.transform.forward, (transform.position - Camera.main.transform.position).normalized);
        if (!triggered && inFrontOfCamera && theAngle > dist)
        {
            triggered = true;
            GetComponent<Animator>().SetBool("Spook", true);
            GetComponent<AudioSource>().Play();
            //play the sound
        }
        if (triggered)
        {
            transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, 400 * Time.deltaTime);
            if (Vector3.Distance(Vector3.zero, transform.position) < 40)
            {
                Application.Quit();
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
        }
    }
}
