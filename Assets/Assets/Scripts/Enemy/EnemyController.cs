using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState : MonoBehaviour
{
    protected EnemyController enemyController;


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
    public EnemyChasing chasingState;
    
    private void StartComponents()
    {
        idleState = GetComponent<EnemyIdle>();
        patrollingState = GetComponent<EnemyPatrolling>();
        chasingState = GetComponent<EnemyChasing>();
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
    }
}
