using UnityEngine;
using UnityEngine.AI;
public class EnemyChasing : EnemyState
{
    public int detectionRadius = 5;

    [SerializeField]
    private Vector3 LastHeardSoundPosition = Vector3.zero;

    NavMeshAgent navMeshAgent;
    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        CheckForSounds();
        navMeshAgent.SetDestination(LastHeardSoundPosition);

    }

    public override void EnterState(EnemyController enemyMovmentController)
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        base.EnterState(enemyMovmentController);
    }

    void CheckForSounds()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            ObjectSound sound = hitCollider.GetComponent<ObjectSound>();
            if (sound != null)
            {
                LastHeardSoundPosition = sound.transform.position;
            }

        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        if (LastHeardSoundPosition != Vector3.zero)
        {
            Gizmos.DrawLine(transform.position, LastHeardSoundPosition);
        }
    }


}