using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarMover : MonoBehaviour
{
    [Header("Define position in polar coords")]
    [SerializeField]
    private bool polar_reposition;
    [SerializeField]
    private Vector3 polarCenter;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float radius;

    [Header("Waypoint system")]
    [SerializeField]
    private float rotate_speed;
    [SerializeField]
    private float approach_speed;
    [SerializeField]
    private bool move_to_waypoint;
    [SerializeField]
    private float waypoint_angle;
    [SerializeField]
    private float waypoint_radius;


    private void LateUpdate()
    {
        transform.LookAt(polarCenter);
        if (!polar_reposition) return;
        if (move_to_waypoint)
        {
            angle = Mathf.Lerp(angle, waypoint_angle, Time.deltaTime * rotate_speed);
            radius = Mathf.Lerp(radius, waypoint_radius, Time.deltaTime * approach_speed);
        }
        float newx = radius * Mathf.Cos(angle);
        float newz = radius * Mathf.Sin(angle);
        transform.position = new Vector3(
            polarCenter.x + newx, 
            polarCenter.y, 
            polarCenter.z + newz
            );
    }

    public void PolarMove(float _angle, float _radius)
    {
        angle += _angle;
        radius += _radius;
    }
    public void PolarWaypointSetAbsolute(float _angle, float _radius)
    {
        waypoint_angle = _angle;
        waypoint_radius = _radius;
    }
    public void PolarWaypointSetRelative(float _angle, float _radius)
    {
        waypoint_angle += _angle;
        waypoint_radius += _radius;
    }
    public void SnapToWaypoint()
    {
        angle = waypoint_angle;
        radius = waypoint_radius;
    }
}
