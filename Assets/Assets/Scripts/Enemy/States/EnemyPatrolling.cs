using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolling : EnemyState
{
    public PatrolPath patrolPath;
    [Range(0.1f, 5)]
    public float speed = 2.5f;
    public float waitTime = 0.5f;
    [SerializeField]
    private bool isWaiting = false;
    [SerializeField]
    Vector2 currentPatrolTarget = Vector2.zero;

    private int currentIndex = -1;

    public int detectionRadius = 5;

    [SerializeField]
    NavMeshAgent navMeshAgent;

    private bool isInitialized = false;

    private void Awake()
    {
        if (patrolPath == null)
        {
            patrolPath = GetComponentInChildren<PatrolPath>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        if (patrolPath.length < 2)
        {
            Debug.LogError("Patrol path is not set up correctly");
            return;
        }
        CheckForSounds();
        if (!isInitialized)
        {
            currentIndex = patrolPath.GetClosestPathPoint(enemyController.transform.position).Index;
            currentPatrolTarget = patrolPath.patrolPoints[currentIndex].position;
            isInitialized = true;
        }
        if (!isWaiting)
        {
            if (Vector2.Distance(enemyController.transform.position, currentPatrolTarget) < 0.1f)
            {
                currentIndex = (currentIndex + 1) % patrolPath.length;
                currentPatrolTarget = patrolPath.patrolPoints[currentIndex].position;
                isWaiting = true;
                StartCoroutine(Wait());
            }
            else
            {
                if (currentPatrolTarget != null)
                    navMeshAgent.SetDestination(currentPatrolTarget);
            }
        }


    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;

    }



    public override void EnterState(EnemyController enemyMovmentController)
    {
        base.EnterState(enemyMovmentController);

    }

    void CheckForSounds()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Sound"))
            {
                ObjectSound sound = hitCollider.GetComponent<ObjectSound>();
                if (sound != null)
                {
                    enemyController.currentTarget = hitCollider.transform;
                    enemyController.transitionToState(enemyController.searchingState);
                }
            }
        }
    }


}

