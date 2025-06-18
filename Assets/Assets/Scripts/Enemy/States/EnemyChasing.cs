using UnityEngine;
using UnityEngine.AI;
public class EnemyChasing : EnemyState
{

    [SerializeField]
    public GameObject target;

    NavMeshAgent navMeshAgent;
    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        // navMeshAgent.SetDestination(LastHeardSoundPosition);
        CheckForPlayer();
        if (target == null)
        {
            enemyController.transitionToState(enemyController.searchingState);
            return;
        }
        navMeshAgent.SetDestination(target.transform.position);
    }

    public override void EnterState(EnemyController enemyMovmentController)
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        base.EnterState(enemyMovmentController);
        enemyController.detectionRadius = 7f; // Set the detection radius for chasing
    }
    void CheckForPlayer()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, enemyController.detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerStateController player = hitCollider.GetComponent<PlayerStateController>();
                if (player != null)
                {
                   
                    target = hitCollider.gameObject;
                    return;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
    }


}