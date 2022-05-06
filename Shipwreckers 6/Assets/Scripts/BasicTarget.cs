using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTarget : MonoBehaviour, IShootable
{
    private Vector3 startSize;
    private Vector3 startPos;
    private void Awake()
    {
        startSize = transform.localScale;
        startPos = transform.position;
    }
    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, startPos, Time.deltaTime*5.0f);
        transform.localScale = Vector3.Lerp(transform.localScale, startSize, Time.deltaTime*1.5f);
    }

    public void Shoot(CannonAmmoConfig cac, Vector3 hitp)
    {
        transform.localScale *= 0.75f;
    }

    private void OnEnable()
    {
        transform.position -= Vector3.up * 50.0f;
    }
}
