using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeItSpin : MonoBehaviour
{
    private Vector3 spin;
    private void Start()
    {
        spin = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
    }
    void FixedUpdate()
    {
        transform.eulerAngles += spin * Time.fixedDeltaTime;
    }
}
