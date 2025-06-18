using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStateController : MonoBehaviour
{
    public PlayerState currentState;
    private PlayerStats playerStats;
    public IdleState idleState;
    public WalkingState walkingState;
    public CrouchState crouchState;
    public DeadState deadState;
    private SpriteRenderer spriteRenderer;


    private float detectionRange = 2F;
    float fovAngle = 45f; // Field of view angle
    float fovDistance = 3f; // Distance of the cone

    private void StartComponents()
    {

        idleState = GetComponent<IdleState>();
        walkingState = GetComponent<WalkingState>();
        crouchState = GetComponent<CrouchState>();
        deadState = GetComponent<DeadState>();
        playerStats = GetComponent<PlayerStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartComponents();
        transitionToState(walkingState);
    }

    void Update()
    {
        currentState.UpdateState();

        UpdateFacingDirection();

        if (Input.GetKeyDown(KeyCode.E))
        {
            checkForInteractable();
        }
        RenderEnemiesInFOV();
        MarkNearbyEnemiesAsSeen();

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);


        Vector2 playerPosition = transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 forwardDirection = (mousePosition - playerPosition).normalized;

        Vector2 leftBoundary = Quaternion.Euler(0, 0, fovAngle / 2) * forwardDirection;
        Vector2 rightBoundary = Quaternion.Euler(0, 0, -fovAngle / 2) * forwardDirection;

        Gizmos.DrawLine(transform.position, transform.position + (Vector3)leftBoundary * fovDistance);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)rightBoundary * fovDistance);
    }

    private void OnGUI()
    {
        string stateName = currentState.GetType().Name;
        GUILayout.Label("Current State: " + stateName);
    }

    public void transitionToState(PlayerState state)
    {
        currentState?.ExitState();
        currentState = state;
        currentState.EnterState(this);
    }


    public void checkForInteractable()
    {

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, LayerMask.GetMask("Interactable"));

        var closestInteractable = hitColliders[0].transform;
        var closestDistance = Vector2.Distance(transform.position, closestInteractable.position);
        foreach (var hitCollider in hitColliders)
        {
            var distance = Vector2.Distance(transform.position, hitCollider.transform.position);
            if (distance < closestDistance)
            {
                closestInteractable = hitCollider.transform;
                closestDistance = distance;
            }
        }
        if (closestInteractable != null)
        {
            closestInteractable.GetComponent<Interactable>().Interact();
        }
    }

    private void UpdateFacingDirection()
    {
        // Get the mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Compare the mouse position with the player's position
        if (mousePosition.x > transform.position.x)
        {
            spriteRenderer.flipX = true; // Facing right
        }
        else if (mousePosition.x < transform.position.x)
        {
            spriteRenderer.flipX = false; // Facing left
        }
    }

    void RenderEnemiesInFOV()
    {
        float fovAngle = 45f; // Field of view angle
        float fovDistance = 10f; // Distance of the cone
        Vector2 playerPosition = transform.position;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 forwardDirection = (mousePosition - playerPosition).normalized;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerPosition, fovDistance);

        foreach (var hitCollider in hitColliders)
        {
            Vector2 directionToTarget = (hitCollider.transform.position - transform.position).normalized;
            float angleToTarget = Vector2.Angle(forwardDirection, directionToTarget);

            if (angleToTarget <= fovAngle / 2 && hitCollider.CompareTag("Enemy"))
            {
                EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    VisibilityManager.ReportSeenEnemy(enemy);
                }
            }
        }

        VisibilityManager.UpdateVisibility();
    }

    void MarkNearbyEnemiesAsSeen()
    {
        Vector2 playerPosition = transform.position;

        // Get all enemies within the proximity radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(playerPosition, detectionRange, LayerMask.GetMask("Enemy"));

        foreach (var hitCollider in hitColliders)
        {
            EnemyController enemy = hitCollider.GetComponent<EnemyController>();
            if (enemy != null)
            {
                VisibilityManager.ReportSeenEnemy(enemy);
            }
        }
        VisibilityManager.UpdateVisibility();
    }
}

public abstract class PlayerState : MonoBehaviour
{
    protected PlayerStateController playerMovmentController;


    public virtual void EnterState(PlayerStateController playerMovmentController)
    {
        this.playerMovmentController = playerMovmentController;
    }

    public abstract void ExitState();
    public abstract void UpdateState();


}

