using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePlane : MonoBehaviour
{
    public static FadePlane fp;
    private MeshRenderer mr;

    private Color icolor;
    private float fadeSpeed = 0.5f;
    private void Awake()
    {
        fp = this;
        mr = GetComponent<MeshRenderer>();
        icolor = mr.material.color;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitThenFade(Color.white));
        //FadeTo(Color.white);
    }
    
    IEnumerator WaitThenFade(Color _to)
    {
        yield return new WaitForSeconds(2.0f);
        FadeTo(_to);
    }
    public void FadeTo(Color _to)
    {
        icolor = _to;
    }
    private void Update()
    {
        mr.material.color = Color.Lerp(mr.material.color, icolor, Time.deltaTime*fadeSpeed);
    }

    public static void Hurt()
    {
        if (fp == null) return;
        fp.mr.material.color = Color.red;
    }
}
