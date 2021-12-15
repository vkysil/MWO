using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script managing moving platform behaviour
public class MovingPlatform : MonoBehaviour
{
    public Transform[] waypoints; // waypoints beetween which the platform moves
    public float movementSpeed = 5f; // platform movement speed
    public Vector2 frameDifference; // movement frame difference

    private Vector3 _lastPosition; // last known platform position
    private Vector3 _currentWaypoint; // current waypoint destination
    private int _waypointCounter; // iterator over all waypoints

    // start is called before the first frame update
    void Start()
    {
        _waypointCounter = 0; // start with the first waypoint
        _currentWaypoint = waypoints[_waypointCounter].position; // set as current
    }

    // Update is called once per frame
    void Update()
    {
        // update platform position
        _lastPosition = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, _currentWaypoint, movementSpeed * Time.deltaTime);

        // if current waypoint is reached, select the next waypoint as new destination
        if(Vector3.Distance(transform.position, _currentWaypoint) < 0.1f)
        {
            _waypointCounter++;
            // reset waypoint back to the first if needed
            if(_waypointCounter >= waypoints.Length)
            {
                _waypointCounter = 0;
            }
            _currentWaypoint = waypoints[_waypointCounter].position;
        }
        frameDifference = transform.position - _lastPosition;
    }
}
