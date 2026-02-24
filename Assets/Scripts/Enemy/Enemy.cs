using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] waypoints;
    private int waypointIndex = 0;
    private Animator animator;
    public float speed = 10f;



    void Start()
    {
        animator = GetComponent<Animator>();
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
        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat("Speed", currentSpeed);

        if (currentSpeed > 0.1f)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isIdle", false);
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdle", true);
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MovetoWaypoints();
        }
        
    }
    


}
