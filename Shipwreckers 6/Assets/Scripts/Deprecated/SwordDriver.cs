using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Deformation;

public class SwordDriver : MonoBehaviour
{
    //Objects & Components:
    private LineRenderer lr;         //Linerenderer representing cutting edge of sword
    private Holsterable holsterable; //This sword's holsterable component
    private AudioSource audioSource; //This sword's audio source component

    //Settings:
    [Header("Bifurcation Settings:")]
    [SerializeField] [Tooltip("Sound made when sword cuts an object")] private AudioClip cutSound;
    [SerializeField] [Tooltip("How much force is applied to object halves when an object is cut")] private float cutSeparationForce;


    //Runtime Vars:
    private Deformable currentTarget;                    //Deformable currently being sliced
    private Vector3[] bladeEntryPoints = new Vector3[2]; //Start and end of blade at beginning of most recent cut

    //RUNTIME METHODS:
    private void Start()
    {
        //Get objects and components:
        lr = GetComponent<LineRenderer>();         //Get linerenderer component
        holsterable = GetComponent<Holsterable>(); //Get holsterable component
        audioSource = GetComponent<AudioSource>(); //Get audio source component
    }
    private void FixedUpdate()
    {
        if (!holsterable.isHolstered) CheckEdgeStatus(); //Check for bifurcations if sword is not in holster
    }

    //PHYSICS METHODS:
    private void CheckEdgeStatus()
    {
        //Function: Draws a ray to see if any objects are currently being intersected by sword

        //Generate linecast:
        Vector3 bladeEndPos = transform.TransformPoint(lr.GetPosition(1)); //Get position of end of blade
        Physics.Linecast(transform.position, bladeEndPos, out RaycastHit hit); //Check for hit

        //Check for start/continuation of cut:
        if (hit.collider?.GetComponent<Deformable>() != null) //Blade has hit a deformable
        {
            if (currentTarget == null) //Blade has just touched a new deformable
            {
                currentTarget = hit.collider.GetComponent<Deformable>(); //Get reference to deformable
                //bladeEntryPoints[0] = transform.position; //Mark beginning position of blade at start of cut
                //bladeEntryPoints[1] = bladeEndPos;        //Mark end position of blade at start of cut
                bladeEntryPoints[0] = currentTarget.transform.position;
                bladeEntryPoints[1] = currentTarget.transform.TransformPoint(lr.GetPosition(1));
                Debug.DrawLine(bladeEntryPoints[0], bladeEntryPoints[1], Color.red, 10);
            }
            return; //As long as blade has touched a deformable, skip rest of check
        }

        //Check for end of cut:
        if (currentTarget != null) //Blade has a current target but is no longer touching it (end of cut)
        {   
            //Create cutting plane:
            //Vector3 side1 = bladeEndPos - bladeEntryPoints[1]; //Get one side of plane-aligning triangle
            //Vector3 side2 = bladeEndPos - bladeEntryPoints[0]; //Get another side of plane-aligning triangle
            Vector3 side1 = currentTarget.transform.TransformPoint(lr.GetPosition(1)) - bladeEntryPoints[1];
            Vector3 side2 = currentTarget.transform.TransformPoint(lr.GetPosition(1)) - bladeEntryPoints[0];
            Vector3 normal = Vector3.Cross(side1, side2).normalized; //Get normal of cutting plane (direction it is facing)
            normal = ((Vector3)(currentTarget.transform.localToWorldMatrix.transpose * normal)).normalized; //Transform normal to align with transform of object being sliced
            Vector3 startingPoint = currentTarget.transform.InverseTransformPoint(bladeEntryPoints[1]);     //Get starting point of cut transformed into local space of object being cut
            Plane cuttingPlane = new Plane(normal, startingPoint); //Create plane using gathered alignment data
            if (normal.x < 0 || normal.y < 0) cuttingPlane.Flip(); //Flip plane if normal is facing downward, in order to ensure plane is always facing straight up

            //Slice and add force:
            GameObject[] slices = Bifurcator.Bifurcate(currentTarget, cuttingPlane);                                   //Perform slicing operation and return new objects
            slices[1].GetComponent<Rigidbody>().AddForce(normal + Vector3.up * cutSeparationForce, ForceMode.Impulse); //Apply cut separation force to upper half in normal direction of cutting plane

            //Effects:
            audioSource.PlayOneShot(cutSound); //Play cut sound effect

            //Cleanup:
            currentTarget = null; //Indicate that blade no longer has target (preventing infinite slicing)
        }
    }
}
