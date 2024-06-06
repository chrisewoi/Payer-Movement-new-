using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWalk : EnemyBase
{
    [Tooltip("How far the enemy can move from its starting point")]
    [SerializeField] protected float wanderDistance = 20f;
    [Tooltip("Should the enemy stop moving when they attack?")]
    [SerializeField] protected bool stopWhileAttacking;

    protected Vector3 homePosition;
    protected Vector3 targetPosition;

    protected NavMeshAgent agent;

    protected bool waiting;

    protected override void Start()
    {
        base.Start();
        homePosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        ChooseNewLocation();
    }

    protected override void Update()
    {
        base.Update();
        // Tell the agent to stop if it's attacking (and it should stop)
        agent.isStopped = (stopWhileAttacking && isAttacking);

        if(agent.remainingDistance <  1.5f && !waiting && !isAttacking)
        {
            StartCoroutine(WaitAndChooseNewLocation());
        }
    }

    IEnumerator WaitAndChooseNewLocation()
    {
        waiting = true;
        yield return new WaitForSeconds(Random.Range(1.5f, 3f));
        ChooseNewLocation();
        waiting = false;
    }

    private void ChooseNewLocation()
    {
        // Pick a random spot inside a circle
        Vector2 randomTarget = Random.insideUnitCircle * wanderDistance;

        // Flatten the circle to a 3D plane
        Vector3 flatTarget = new Vector3(
            homePosition.x + randomTarget.x,
            homePosition.y,
            homePosition.z + randomTarget.y
            );

        // Raycast down and try to find navmesh there
        if (!Physics.Raycast(flatTarget + Vector3.up * 20f, Vector3.down,out RaycastHit hit, 40f))
        {
            ChooseNewLocation();
            return;
        }

        // Check if we have a complete path
        NavMeshPath path = new();

        if (!agent.CalculatePath(hit.point, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            ChooseNewLocation();
            return;
        }

        // Update our target if we found a path
        targetPosition = hit.point;
        agent.SetDestination(targetPosition);

    }
}
