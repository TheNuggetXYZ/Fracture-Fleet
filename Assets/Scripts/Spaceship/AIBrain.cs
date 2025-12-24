using System;
using UnityEngine;

public class AIBrain : MonoBehaviour, IShootInput
{
    [Header("General")]
    [field: SerializeField] public Rigidbody rb { get; private set; }
    [SerializeField] private Transform target;
    [SerializeField] private bool findPlayer = true;

    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleClearance = 10f;
    [SerializeField] private float obstacleAvoidAheadMultiplier = 0.3f;

    [Header("Avoid others")] 
    [SerializeField] private LayerMask otherAIMask;
    [SerializeField] private float otherAICheckRadius = 30f;
    [SerializeField] private float repelFromOthersForce = 1f;
    [SerializeField] private float repelLerpSpeed = 3f;
    [SerializeField] private float repelForceOnRotationMultiplier = 0.5f;
    
    [Header("Shooting")] 
    [SerializeField] private float differenceInDirectionThatAllowsShooting = 0.95f;
    [SerializeField] private float startShootingRatePerMinute = 20;
    [SerializeField] private float stopShootingRatePerMinute = 10;
    
    [Header("Zooming")]
    [SerializeField, Tooltip("Controls how much the AI will zoom diagonally instead of straight to the target")]
    private float zoomOffsetFromPlayerPerPlayerDistance = 0.3f;
    [SerializeField, Tooltip("How much it zooms past the target")] 
    private float zoomExtendedDistance = 120;
    [SerializeField, Tooltip("How close the zoom target position has to be for the AI to stop zooming")] 
    private float zoomTargetStateExitDistance = 20;
    [SerializeField, Tooltip("The AI will not zoom inside when the distance to player is inside this range, but it must zoom when outside.")]
    private Vector2 noZoomDistanceRange;
    
    [Header("Following")]
    [SerializeField] private float followDistance;
    
    
    [Header("Output")] 
    public Action AfterBrainUpdate;
    public Quaternion targetRotation { get; private set; }
    public int speedTier { get; private set; }
    public AIState currentState { get; private set; }
    

    [Header("Other")] 
    private Vector3 previousRepelVector;
    private bool shouldShoot;
    private bool wantsToShoot;
    private Vector3 zoomTargetPosition;
    private Vector3 zoomTargetPositionLocalToTarget;
    private Vector3 targetPosForGizmos;

    private bool shipDied;
    
    GameManager game;
    
    public enum AIState
    {
        following = 0,
        zoomingPast = 1,
        idle = 2,
    }

    private void Awake()
    {
        game = GameManager.I;
        
        if (findPlayer)
            target = game.player.transform;
    }

    private void Update()
    {
        zoomTargetPosition = zoomTargetPositionLocalToTarget + target.position;
        
        Think(out var targetPosition, out var upVector, out var repelVector);
        
        CalculateRotation(targetPosition, upVector, repelVector);
        CalculateMovement();
        
        AfterBrainUpdate?.Invoke();
        targetPosForGizmos = targetPosition;
    }

    private void Think(out Vector3 targetPosition, out Vector3 newTransformUp, out Vector3 repelVector)
    {
        DetectObstacle(out bool foundObstacle, out targetPosition, out newTransformUp);
        
        RepelFromOthers(foundObstacle, newTransformUp, out repelVector);

        DecideShooting(foundObstacle);
        
        ControlState(ref targetPosition, ref newTransformUp, foundObstacle);
    }

    private void ControlState(ref Vector3 targetPosition, ref Vector3 newTransformUp, bool foundObstacle)
    {
        float playerDistance = Vector3.Distance(transform.position, target.position);
        
        if (!foundObstacle)
        {
            // TODO: if youre lucky enough, the AI can just zoom from left to right for a minute, add a cooldown
            bool shouldStartZooming = playerDistance < noZoomDistanceRange.x || playerDistance > noZoomDistanceRange.y;
        
            
            if (currentState != AIState.zoomingPast && shouldStartZooming)
            {
                StartZooming(playerDistance);
            }
            else if (currentState == AIState.zoomingPast)
            {
                ZoomingUpdate(ref targetPosition, ref newTransformUp);
            }
            else if (playerDistance > followDistance)
            {
                currentState = AIState.following;
            }
            else
            {
                currentState = AIState.idle;
            }
        }
        else
            currentState = AIState.following;
    }

    private void ZoomingUpdate(ref Vector3 targetPosition, ref Vector3 newTransformUp)
    {
        if (Vector3.Distance(transform.position, zoomTargetPosition) < zoomTargetStateExitDistance)
            currentState = AIState.following;
        else
        {
            targetPosition = zoomTargetPosition;
            newTransformUp = transform.position - target.position;
        }
    }

    private void StartZooming(float playerDistance)
    {
        currentState = AIState.zoomingPast;
            
        /*Vector3 playerDirection = target.position - transform.position;
        Vector3 directionTheShipIsFacingFromThePlayer = Vector3.ProjectOnPlane(transform.forward, playerDirection).normalized;

        Vector3 offsetedPositionFromPlayerWhereTheShipIsFacingMore = target.position + Utils.ResizeVector(directionTheShipIsFacingFromThePlayer, zoomOffsetFromPlayerPerPlayerDistance * playerDistance);
        Vector3 targetWorldPosition = transform.position + Utils.ExtendVector(offsetedPositionFromPlayerWhereTheShipIsFacingMore - transform.position, zoomExtendedDistance);
        // TODO: make it be local in a way it rotates around, if you go to the opposite of the AI the zoom target will be much closer and in a direction that's still as if the player was on the original position
        zoomTargetPositionLocalToPlayer = targetWorldPosition - target.position;*/

        zoomTargetPositionLocalToTarget = RandomPointInFollowSphere() - target.position;

        Vector3 RandomPointInFollowSphere()
        {
            Vector3 point = Utils.RandomPointInSphere(target.position, noZoomDistanceRange.y);
            if (Vector3.Distance(target.position, point) > followDistance)
                return point;
            else
                return RandomPointInFollowSphere();
        }
    }

    private void DecideShooting(bool foundObstacle)
    {
        if (Utils.RandomEventInTime(startShootingRatePerMinute))
            wantsToShoot = true;
        else if (Utils.RandomEventInTime(stopShootingRatePerMinute))
            wantsToShoot = false;
        
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

    private void CalculateRotation(Vector3 targetPosition, Vector3 upVector, Vector3 repelVector)
    {
        Vector3 targetDirection = targetPosition - transform.position;

        Vector3 combined = (targetDirection.normalized + Vector3.ClampMagnitude(repelVector, 1) * repelForceOnRotationMultiplier).normalized;

        if (combined == Vector3.zero)
            combined = transform.forward;

        targetRotation = Quaternion.LookRotation(combined, upVector);
    }

    private void CalculateMovement()
    {
        if (currentState == AIState.following)
        {
            speedTier = 1;
        } 
        else if (currentState == AIState.zoomingPast)
        {
            speedTier = 2;
        }
        else
            speedTier = 0;
    }

    public bool IsShooting() => shouldShoot && wantsToShoot;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPosForGizmos);
    }

    public void ShipDied()
    {
        Destroy(this);
    }
}