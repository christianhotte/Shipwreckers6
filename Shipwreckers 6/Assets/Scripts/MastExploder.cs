using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MastExploder : MonoBehaviour
{
    public static List<MastExploder> masts = new List<MastExploder>();
    private Rigidbody rb;

    public GameObject explosionPrefab;
    public Transform explodePoint;
    public float explodeForce;
    public float maxExlpodeDistance;

    internal bool explodable = false;

    public bool debugExplode;

    private void Awake()
    {
        masts.Add(this);
        rb = GetComponent<Rigidbody>();
        if (name.Contains("Top") && !name.Contains("Mid")) explodable = true;
    }
    private void Update()
    {
        if (debugExplode) { ExplodeNextMast(); debugExplode = false; }
    }
    private void OnDisable()
    {
        masts.Remove(this);
    }

    public void Explode()
    {
        GameObject explodeEffect = Instantiate(explosionPrefab);
        explodeEffect.transform.parent = transform.parent;
        if (transform.parent.TryGetComponent(out MastExploder mastBottom))
        {
            mastBottom.explodable = true; //Make parent explodable
            explodeEffect.transform.parent = explodeEffect.transform.parent.parent; //Make sure explosion is childed to ship model
        }
        explodeEffect.transform.position = explodePoint.position;
        explodeEffect.transform.localScale *= 4;

        transform.parent = null;
        rb.isKinematic = false;
        Vector2 explodeHoriz = Random.insideUnitCircle.normalized * maxExlpodeDistance;
        Vector3 explodeDirection = new Vector3(explodeHoriz.x, explodeForce, explodeHoriz.y);
        rb.AddForceAtPosition(explodeDirection, explodePoint.position, ForceMode.Impulse);

        masts.Remove(this);

        if (masts.Count == 2) foreach (MastExploder mast in masts) if (mast.name.Contains("Top")) mast.explodable = true; //The supremest of jank ways to make mid top explode fifth

        Destroy(gameObject, 10f);
    }
    public static void ExplodeNextMast()
    {
        if (masts.Count == 0) { Debug.Log("Out of masts to explode"); return; }
        List<MastExploder> tempMasts = new List<MastExploder>(masts);
        while (tempMasts.Count > 0)
        {
            MastExploder mast = tempMasts[Random.Range(0, tempMasts.Count)]; //Get random mast from remaining masts
            if (mast.explodable)
            {
                mast.Explode();
                print("Mast exploded!");
                break;
            }
            tempMasts.Remove(mast);
        }
    }
}
