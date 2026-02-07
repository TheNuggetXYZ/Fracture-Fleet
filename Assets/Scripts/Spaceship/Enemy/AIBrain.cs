using System;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour, IShootInput
{
    [Header("General")]
    [field: SerializeField] public Rigidbody rb { get; private set; }
    [SerializeField] private bool findTarget = true;
    [SerializeField] private SpaceshipPartManager currentTarget;
    [SerializeField] private SpaceshipPartManager spaceshipManager;

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
    [SerializeField, Tooltip("How close the zoom target position has to be for the AI to stop zooming")] 
    private float zoomTargetStateExitDistance = 20;
    [SerializeField, Tooltip("The AI will not zoom inside when the distance to player is inside this range, but it must zoom when outside.")]
    private Vector2 noZoomDistanceRange;
    
    [Header("Following")]
    [SerializeField] private float followDistance;
    
    
    [Header("Output")] 
    public Quaternion targetRotation { get; private set; }
    public int speedTier { get; private set; }
    public float rotationMultiplier { get; private set; }
    public float speedMultiplier { get; private set; }
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
        zooming = 1,
        idle = 2,
    }

    private void Awake()
    {
        game = GameManager.I;
    }

    private void Start()
    {
        DecideTarget();
    }

    private void OnEnable()
    {
        game.hierarchyManager.OnEnemiesChanged += DecideTargetIfNullOrDead;
        game.hierarchyManager.OnCreatedShipsChanged += DecideTargetIfNullOrDead;
    }

    private void OnDisable()
    {
        game.hierarchyManager.OnEnemiesChanged -= DecideTargetIfNullOrDead;
        game.hierarchyManager.OnCreatedShipsChanged -= DecideTargetIfNullOrDead;
    }

    private void Update()
    {
        if (currentTarget)
            zoomTargetPosition = zoomTargetPositionLocalToTarget + currentTarget.transform.position;
        
        Think(out var targetPosition, out var upVector, out var repelVector);
        
        CalculateRotation(targetPosition, upVector, repelVector);
        CalculateSpeed();
        CalculateMultipliersForPreciseRotation();
        
        targetPosForGizmos = targetPosition;
    }

    private void Think(out Vector3 targetPosition, out Vector3 newTransformUp, out Vector3 repelVector)
    {
        targetPosition = transform.position;
        newTransformUp = transform.up;
        repelVector = Vector3.zero;
        
        if (!currentTarget)
        {
            currentState = AIState.idle;
            return;
        }
        if (currentTarget.shipDead)
        {
            currentState = AIState.idle;
            DecideTarget();
            return;
        }
            
        DetectObstacle(out bool foundObstacle, out targetPosition, out newTransformUp);
        
        RepelFromOthers(foundObstacle, newTransformUp, out repelVector);

        DecideShooting(foundObstacle);
        
        ControlState(ref targetPosition, ref newTransformUp, foundObstacle);
    }

    private void ControlState(ref Vector3 targetPosition, ref Vector3 newTransformUp, bool foundObstacle)
    {
        float playerDistance = Vector3.Distance(transform.position, currentTarget.transform.position);
        
        if (foundObstacle)
        {
            currentState = AIState.following;
            return;
        }

        if (currentState != AIState.zooming && !Utils.IsInRange(playerDistance, noZoomDistanceRange))
        {
            currentState = AIState.zooming;
            zoomTargetPositionLocalToTarget = RandomPointInFollowSphere() - currentTarget.transform.position;
        }
        else if (currentState == AIState.zooming && Vector3.Distance(transform.position, zoomTargetPosition) < zoomTargetStateExitDistance)
        {
            currentState = AIState.following;
        }
        else if (currentState != AIState.zooming && playerDistance > followDistance)
        {
            currentState = AIState.following;
        }
        else if (currentState != AIState.zooming)
            currentState = AIState.idle;
        
        if (currentState == AIState.zooming)
        {
            targetPosition = zoomTargetPosition;
            newTransformUp = transform.position - currentTarget.transform.position;
        }
    }

    private void CalculateMultipliersForPreciseRotation()
    {
        if (currentState != AIState.zooming)
        {
            rotationMultiplier = 1f;
            speedMultiplier = 1f;
        }
        
        Vector3 toZoomTarget = (zoomTargetPosition - transform.position).normalized;
        Vector3 currentDirection = transform.forward;
        
        float dot = Vector3.Dot(toZoomTarget, currentDirection);
        
        float rotationAmount = (-dot + 1) / 2;
        const float rotationBoost = 2f;
        rotationMultiplier = rotationAmount * rotationBoost;
        
        speedMultiplier = (dot + 1) / 2;
    }
    
    private Vector3 RandomPointInFollowSphere()
    {
        Vector3 point = Utils.RandomPointInSphere(currentTarget.transform.position, noZoomDistanceRange.y);
        if (Vector3.Distance(currentTarget.transform.position, point) > followDistance)
            return point;
        else
            return RandomPointInFollowSphere();
    }

    private void DecideShooting(bool foundObstacle)
    {
        if (Utils.RandomEventInTime(startShootingRatePerMinute))
            wantsToShoot = true;
        else if (Utils.RandomEventInTime(stopShootingRatePerMinute))
            wantsToShoot = false;
        
        if (foundObstacle || currentState == AIState.zooming)
        {
            shouldShoot = false;
            return;
        }
        
        Vector3 forward = transform.forward;
        Vector3 toTarget = (currentTarget.transform.position - transform.position).normalized;

        shouldShoot = Vector3.Dot(forward, toTarget) > differenceInDirectionThatAllowsShooting;
    }

    private void RepelFromOthers(bool foundObstacle, Vector3 obstacleNormal, out Vector3 repelVector)
    {
        repelVector = Vector3.zero;

        if (currentState == AIState.idle)
            return; // no need for repelling
        
        // repel from other AIs
        Collider[] others = Physics.OverlapSphere(transform.position, otherAICheckRadius, otherAIMask);

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
        targetPosition = currentTarget.transform.position;
        upVector = currentTarget.transform.up;
        
        Vector3 toTarget;
        if (currentState == AIState.zooming)
            toTarget = zoomTargetPosition - transform.position;
        else
            toTarget = currentTarget.transform.position - transform.position;
        
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

    private void DecideTargetIfNullOrDead()
    {
        if (!currentTarget || currentTarget.shipDead)
            DecideTarget();
    }

    private void DecideTarget()
    {
        float bestScore = -1;
        
        foreach (SpaceshipPartManager ship in game.spaceshipTracker.shipList)
        {
            ComputePotentialTargetScoreAndAssignCurrentTarget(ship, ref bestScore);
        }
        
        ComputePotentialTargetScoreAndAssignCurrentTarget(game.player.GetComponent<SpaceshipPartManager>(), ref bestScore);

        if (bestScore == -1)
            currentTarget = null;
    }

    private void ComputePotentialTargetScoreAndAssignCurrentTarget(SpaceshipPartManager ship, ref float bestScore)
    {
        if (!ship || ship.shipDead || spaceshipManager.shipType == ship.shipType || (spaceshipManager.shipType == SpaceshipPartManager.ShipType.comrade && ship.shipType == SpaceshipPartManager.ShipType.player))
            return;
            
        float score = 1 / Vector3.Distance(transform.position, ship.transform.position);

        if (score > bestScore)
        {
            bestScore = score;
            currentTarget = ship;
        }
    }

    private void CalculateRotation(Vector3 targetPosition, Vector3 upVector, Vector3 repelVector)
    {
        Vector3 targetDirection = targetPosition - transform.position;

        Vector3 combined = (targetDirection.normalized + Vector3.ClampMagnitude(repelVector, 1) * repelForceOnRotationMultiplier).normalized;

        if (combined == Vector3.zero)
            combined = transform.forward;

        targetRotation = Quaternion.LookRotation(combined, upVector);
    }

    private void CalculateSpeed()
    {
        speedTier = currentState switch
        {
            AIState.following => 1,
            AIState.zooming => 2,
            _ => 0
        };
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