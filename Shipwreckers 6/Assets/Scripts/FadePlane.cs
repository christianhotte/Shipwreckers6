using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePlane : MonoBehaviour
{
    public static FadePlane fp;
    private MeshRenderer mr;
    private void Awake()
    {
        fp = this;
        mr = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        FadeTo(Color.white);
    }

    private Color icolor;
    private float fadeSpeed = 0.5f;
    public void FadeTo(Color _to)
    {
        icolor = _to;
    }
    private void Update()
    {
        mr.material.color = Color.Lerp(mr.material.color, icolor, Time.deltaTime*fadeSpeed);
    }
}
