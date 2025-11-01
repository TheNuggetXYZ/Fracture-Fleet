using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIBrain : MonoBehaviour, IShootInput
{
    [Header("General")]
    [field: SerializeField] public Rigidbody rb { get; private set; }
    [SerializeField] private Transform target;
    [SerializeField] private bool findPlayer = true;

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleClearance = 7f;
    [SerializeField] private float obstacleAvoidAheadMultiplier = 0.25f;

    [Header("Avoid others")] 
    [SerializeField] private LayerMask otherAIMask;
    [SerializeField] private float otherAICheckRadius = 30f;
    [SerializeField] private float repelFromOthersForce = 1f;
    [SerializeField] private float repelLerpSpeed = 3f;
    [SerializeField] private float repelForceOnRotationMultiplier = 0.5f;
    
    [Header("Shooting")] 
    [SerializeField] private float differenceInDirectionThatAllowsShooting = 0.9f;
    
    [Header("Movement")]
    [SerializeField] private float zoomPastRatePerMinute;
    [SerializeField] private float zoomOffsetFromPlayerPerPlayerDistance;
    [SerializeField] private float zoomExtendedDistance;
    [SerializeField] private float zoomTargetStateExitDistance;
    
    
    [Header("Output (readonly)")] 
    public Action AfterBrainUpdate;
    public Quaternion targetRotation { get; private set; }
    public AIState currentState { get; private set; }

    private Vector3 previousRepelVector;
    private bool shouldShoot;
    private Vector3 zoomTargetPositionLocalToPlayer;
    private Vector3 zoomTargetPosition;
    private Vector3 targetPosForGizmos;
    
    public enum AIState
    {
        following = 0,
        zoomingPast = 1,
    }

    private void Start()
    {
        if (findPlayer)
        {
            target = GameManager.I.player.transform;
        }
    }

    private void Update()
    {
        zoomTargetPosition = zoomTargetPositionLocalToPlayer + target.position;
        
        Think(out var targetPosition, out var upVector, out var repelVector);
        CalculateRotation(targetPosition, upVector, repelVector);
        AfterBrainUpdate?.Invoke();
        targetPosForGizmos = targetPosition;
    }

    private void CalculateRotation(Vector3 targetPosition, Vector3 upVector, Vector3 repelVector)
    {
        Vector3 targetDirection = targetPosition - transform.position;

        Vector3 combined = (targetDirection.normalized + Vector3.ClampMagnitude(repelVector, 1) * repelForceOnRotationMultiplier).normalized;

        if (combined == Vector3.zero)
            combined = transform.forward;

        targetRotation = Quaternion.LookRotation(combined, upVector);
    }

    private void Think(out Vector3 targetPosition, out Vector3 newTransformUp, out Vector3 repelVector)
    {
        DetectObstacle(out bool foundObstacle, out targetPosition, out newTransformUp);
        
        RepelFromOthers(foundObstacle, newTransformUp, out repelVector);

        DecideShooting(foundObstacle);

        DecideZooming(ref targetPosition, ref newTransformUp, foundObstacle);
    }

    private void DecideZooming(ref Vector3 targetPosition, ref Vector3 newTransformUp, bool foundObstacle)
    {
        if (!foundObstacle && currentState != AIState.zoomingPast && Random.value < zoomPastRatePerMinute / 60 * Time.deltaTime)
        {
            Debug.Log("zoom " + gameObject.name);
            
            float playerDistance = Vector3.Distance(transform.position, target.position);
            if (playerDistance > zoomTargetStateExitDistance)
            {
                currentState = AIState.zoomingPast;

                Vector3 playerDirection = target.position - transform.position;
                Vector3 directionOfThePlayerWeAreFacing =
                    Vector3.ProjectOnPlane(transform.forward, playerDirection).normalized;
                zoomTargetPositionLocalToPlayer = target.position + Utils.ResizeVector(directionOfThePlayerWeAreFacing, zoomOffsetFromPlayerPerPlayerDistance * playerDistance);
                zoomTargetPositionLocalToPlayer = transform.position + Utils.ExtendVector(zoomTargetPositionLocalToPlayer - transform.position, zoomExtendedDistance);
                zoomTargetPositionLocalToPlayer = zoomTargetPositionLocalToPlayer - target.position;
            }
        }
        else if (!foundObstacle && currentState == AIState.zoomingPast)
        {
            if (Vector3.Distance(transform.position, zoomTargetPosition) < zoomTargetStateExitDistance)
                currentState = AIState.following;
            else
            {
                targetPosition = zoomTargetPosition;
                newTransformUp = transform.position - target.position;
            }
        }
    }

    private void DecideShooting(bool foundObstacle)
    {
        if (foundObstacle || currentState == AIState.zoomingPast)
        {
            shouldShoot = false;
            return;
        }
        
        Vector3 forward = transform.forward;
        Vector3 toTarget = (target.position - transform.position).normalized;

        shouldShoot = Vector3.Dot(forward, toTarget) > differenceInDirectionThatAllowsShooting;
    }

    private void RepelFromOthers(bool foundObstacle, Vector3 obstacleNormal, out Vector3 repelVector)
    {
        // repel from other AIs
        Collider[] others = Physics.OverlapSphere(transform.position, otherAICheckRadius, otherAIMask);
        repelVector = Vector3.zero;

        // sum up directions from everyone
        foreach (Collider other in others)
        {
            if (other.attachedRigidbody == rb) continue;

            Vector3 away = transform.position - other.transform.position;
            float dist = away.magnitude;
            if (dist == 0f) continue;

            float strength = Mathf.Clamp01(1f - dist / otherAICheckRadius); // close = strong
            repelVector += away.normalized * (strength * repelFromOthersForce);
        }

        repelVector = Vector3.Lerp(previousRepelVector, repelVector, repelLerpSpeed * Time.deltaTime);

        // don't make ships repel into the obstacle
        if (foundObstacle)
        {
            // Only apply correction if repel is pointing into the obstacle
            if (Vector3.Dot(repelVector, -obstacleNormal) > 0f)
            {
                // Remove the "into" component
                repelVector = Vector3.ProjectOnPlane(repelVector, -obstacleNormal);
            }
        }
        
        previousRepelVector = repelVector;
    }

    private void DetectObstacle(out bool foundObstacle, out Vector3 targetPosition, out Vector3 upVector)
    {
        // Default values
        targetPosition = target.position;
        upVector = target.up;
        
        Vector3 toTarget;
        if (currentState == AIState.zoomingPast)
            toTarget = zoomTargetPosition - transform.position;
        else
            toTarget = target.position - transform.position;
        
        float distance = toTarget.magnitude;

        // Obstacle avoidance
        if (Physics.Raycast(transform.position, toTarget.normalized, out var hit, distance, obstacleMask))
        {
            Vector3 obstacleCenterDir = hit.point - hit.transform.position;

            targetPosition = hit.transform.position + Utils.ExtendVector(obstacleCenterDir, hit.distance * obstacleAvoidAheadMultiplier + obstacleClearance);

            upVector = obstacleCenterDir.normalized;
            foundObstacle = true;
        }
        else
            foundObstacle = false;

        if (foundObstacle)
            currentState = AIState.following;
    }

    public bool IsShooting() => shouldShoot;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPosForGizmos);
    }
}