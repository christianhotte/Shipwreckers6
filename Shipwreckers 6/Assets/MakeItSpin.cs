using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeItSpin : MonoBehaviour
{
    private Vector3 spin;
    private void Start()
    {
        spin = new Vector3(0, Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        spin = spin.normalized * 250;
    }
    void FixedUpdate()
    {
        transform.eulerAngles += spin * Time.fixedDeltaTime;
    }
}
