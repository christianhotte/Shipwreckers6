using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkRed : MonoBehaviour
{
    private float redTime;
    private MeshRenderer mr;
    private Material mat;

    // Start is called before the first frame update
    void Start()
    {
        redTime = 0.0f;
        mr = GetComponent<MeshRenderer>();
        mat = mr.material;
    }

    // Update is called once per frame
    void Update()
    {
        redTime += Time.deltaTime;
        mat.color = new Color( Mathf.Sin(redTime), 1.0f, 1.0f );
    }
}
