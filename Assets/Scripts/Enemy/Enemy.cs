using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] waypoints;
    private int waypointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        
    }


    public void MovetoWaypoints()
    {
        if (waypoints.Length == 0)
                return;
            agent.destination = waypoints[waypointIndex].position;
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }


    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        MovetoWaypoints();
    }
    


}
