using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointTrack : MonoBehaviour
{
    public Color lineColor = Color.yellow;
    public Transform[] waypoints;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        waypoints = this.GetComponentsInChildren<Transform>();

        int nextIndex = 1;
        Vector3 currentPosition = waypoints[nextIndex].position;
        Vector3 nextPosition;

        for(int i=0; i<waypoints.Length; i++)
        {
            nextPosition = (++nextIndex >= waypoints.Length) ? waypoints[i].position : waypoints[nextIndex].position;
            
            Gizmos.DrawLine(currentPosition, nextPosition);

            currentPosition = nextPosition;
        }
    }
}
