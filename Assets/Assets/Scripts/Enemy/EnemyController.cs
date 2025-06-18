using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState : MonoBehaviour
{
    protected EnemyController enemyController;
    [SerializeField]

    public virtual void EnterState(EnemyController enemyController)
    {
        this.enemyController = enemyController;
    }

    public abstract void ExitState();
    public abstract void UpdateState();

}

public class EnemyController : MonoBehaviour
{
    NavMeshAgent navMeshAgent;

    public EnemyState currentState;
    public EnemyIdle idleState;
    public EnemyPatrolling patrollingState;

    public Transform currentTarget;

    public EnemySearching searchingState;
    public EnemyChasing chasingState;

    public bool beingSeen = false;

    public float detectionRadius = 2f; // Default detection radius, can be overridden in derived classes

    private void StartComponents()
    {
        idleState = GetComponent<EnemyIdle>();
        patrollingState = GetComponent<EnemyPatrolling>();
        chasingState = GetComponent<EnemyChasing>();
        searchingState = GetComponent<EnemySearching>();
    }

    void Start()
    {
        StartComponents();
        transitionToState(patrollingState);
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
    }

    public void transitionToState(EnemyState state)
    {
        currentState?.ExitState();
        currentState = state;
        currentState.EnterState(this);
    }




    void Update()
    {
        currentState.UpdateState();
        if (beingSeen)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }



        VisibilityManager.ReportUnseenEnemy(gameObject.GetComponent<EnemyController>());


        VisibilityManager.UpdateVisibility();

        Vector3 velocity = navMeshAgent.velocity;
        if (velocity.x != 0) // Check if moving horizontally
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = velocity.x > 0; // Flip if moving left
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
