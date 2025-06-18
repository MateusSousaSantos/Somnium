using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemySearching : EnemyState
{

    private NavMeshAgent navMeshAgent;
    public override void EnterState(EnemyController enemyController)
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyController.detectionRadius = 6f;
        base.EnterState(enemyController);

        navMeshAgent.SetDestination(enemyController.currentTarget.position);
    }
    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        if(enemyController.currentTarget == null)
        {
            enemyController.transitionToState(enemyController.patrollingState);
            return;
        }
        else if (Vector2.Distance(transform.position, enemyController.currentTarget.position) < 0.1f)
        {
            CheckForPlayer();
        }
        navMeshAgent.SetDestination(enemyController.currentTarget.position);
    }

    void CheckForPlayer()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 5f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerStateController player = hitCollider.GetComponent<PlayerStateController>();
                if (player != null)
                {
                    enemyController.transitionToState(enemyController.chasingState);
                    return;
                }

            }
            else
            {
                enemyController.transitionToState(enemyController.patrollingState);
            }
        }
    }




}
