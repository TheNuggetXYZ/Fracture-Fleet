using UnityEngine;

public class AISpaceshipController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform target;
    [SerializeField] private bool findPlayer = true;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float rotationLerpSpeed = 3f;
    
    [Header("Obstacles")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleClearance = 7f;
    [SerializeField] private float obstacleAvoidAheadMultiplier = 0.25f;
    
    [Header("Avoid others")]
    [SerializeField] private LayerMask otherAIMask;
    [SerializeField] private float otherAICheckRadius = 30f;
    [SerializeField] private float repelFromOthersForce = 1f;
    [SerializeField] private float repelLerpSpeed = 3f;

    private Vector3 previousRepelVector;

    [Header("Debug")] 
    public bool move = true;
    public float repelMagnitude;
    public float repelClampedMagnitude;

    private void Update()
    {
        DetectObstacles(out var targetPosition, out var upVector, out var repelVector);
        UpdateRotation(targetPosition, upVector, repelVector);
    }

    private void FixedUpdate()
    {
        if (move)
            rb.AddForce(transform.forward * speed, ForceMode.Force);
    }

    private void UpdateRotation(Vector3 targetPosition, Vector3 upVector, Vector3 repelVector)
    {
        Vector3 targetDirection = targetPosition - transform.position;
        // Weight repel force less than target direction for stability
        Vector3 combined = (targetDirection.normalized + Vector3.ClampMagnitude(repelVector, 1) * 0.5f).normalized;

        if (combined == Vector3.zero)
            combined = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(combined, upVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    private void DetectObstacles(out Vector3 targetPosition, out Vector3 upVector, out Vector3 repelVector)
    {
        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        // 🔸 Obstacle avoidance
        if (Physics.Raycast(transform.position, toTarget.normalized, out var hit, distance, obstacleMask))
        {
            Vector3 obstacleCenterDir = (hit.point - hit.transform.position);
            Vector3 obstacleCenterDirNormalized = obstacleCenterDir.normalized;
            float obstacleCenterDirMagnitude = obstacleCenterDir.magnitude;
            
            targetPosition = hit.transform.position + obstacleCenterDirNormalized * (obstacleCenterDirMagnitude + (hit.distance * obstacleAvoidAheadMultiplier) + obstacleClearance);
            
            upVector = obstacleCenterDir.normalized;
            repelVector = Vector3.Lerp(previousRepelVector, Vector3.zero, repelLerpSpeed * Time.deltaTime);
            previousRepelVector = repelVector;
            return;
        }

        // 🔸 Repel from other AIs
        Collider[] others = Physics.OverlapSphere(transform.position, otherAICheckRadius, otherAIMask);
        repelVector = Vector3.zero;

        foreach (Collider other in others)
        {
            if (other.attachedRigidbody == rb) continue;

            Vector3 away = transform.position - other.transform.position;
            float dist = away.magnitude;
            if (dist == 0f) continue;

            float strength = Mathf.Clamp01(1f - dist / otherAICheckRadius); // close = strong
            repelVector += away.normalized * (strength * repelFromOthersForce);
        }

        repelClampedMagnitude = repelVector.magnitude;
        repelMagnitude = repelVector.sqrMagnitude;

        targetPosition = target.position;
        upVector = target.up;
        
        repelVector = Vector3.Lerp(previousRepelVector, repelVector, repelLerpSpeed * Time.deltaTime);
        previousRepelVector = repelVector;
    }
}


/*using System;
using UnityEngine;

public class AISpaceshipController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask otherAIMask;
    [SerializeField] private float obstacleClearance;
    [SerializeField] private float speed;
    [SerializeField] private float rotationLerpSpeed;
    [SerializeField] private float otherAICheckRadius;
    [SerializeField] private float repelFromOthersForce;
    [SerializeField] private Rigidbody rb;

    [Header("Debug")] 
    public bool move;
    public float repelMagnitude;
    public float repelClampedMagnitude;

    private Vector3 lastTransformUp;

    private void Start()
    {
        lastTransformUp = transform.up;
    }

    private void Update()
    {
        DetectObstacles(out var targetPosition, out var obstacleUpwardsVector, out var repelFromOthersVector);
        
        UpdateRotation(targetPosition, obstacleUpwardsVector, repelFromOthersVector);
    }

    private void FixedUpdate()
    {
        if (move)
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        rb.AddForce(transform.forward * speed, ForceMode.Force);
    }

    private void UpdateRotation(Vector3 targetPosition, Vector3 obstacleUpwardsVector, Vector3 repelVector)
    {
        Vector3 rotationDirection = (targetPosition - transform.position) + repelVector;
        Quaternion targetRotation = Quaternion.LookRotation(rotationDirection.normalized, obstacleUpwardsVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed);
    }

    private void DetectObstacles(out Vector3 targetPosition, out Vector3 obstacleUpwardsVector, out Vector3 repelFromOthersVector)
    {
        Vector3 playerDirection = target.position - transform.position;
        float playerDistance = Vector3.Distance(transform.position, target.position);

        Physics.Raycast(transform.position, playerDirection, out var obstacleHit, playerDistance, obstacleMask,
            QueryTriggerInteraction.Ignore);

        if (obstacleHit.transform is not null)
        {
            Vector3
                ObstacleNormalDirection =
                    obstacleHit.point - obstacleHit.transform
                        .position; // direction from center to hit which is more precise than a normal on rugged surfaces
            Vector3 ObstacleNormal = ObstacleNormalDirection.normalized;
            float ObstacleNormalMagnitude = ObstacleNormalDirection.magnitude;
            Vector3 ObstacleNormalDirectionWithClearance =
                ObstacleNormal * (ObstacleNormalMagnitude + obstacleClearance);

            targetPosition = obstacleHit.transform.position + ObstacleNormalDirectionWithClearance;
            obstacleUpwardsVector = (transform.position - obstacleHit.transform.position).normalized;
            repelFromOthersVector = Vector3.zero;

            return;
        }

        Collider[] othersCheckHits = Physics.OverlapSphere(transform.position, otherAICheckRadius, otherAIMask); //(transform.position, otherAICheckRadius, Vector3.zero, 0, otherAIMask, QueryTriggerInteraction.Ignore);
        
        if (othersCheckHits.Length > 0)
        {
            Vector3 getOutOfWayDirection = Vector3.zero;
            foreach (Collider otherCheckHit in othersCheckHits)
            {
                getOutOfWayDirection += transform.position - otherCheckHit.transform.position;
            }

            float clampedMagnitude = Mathf.Clamp(getOutOfWayDirection.magnitude, 0f, otherAICheckRadius);
            if (clampedMagnitude != 0f)
                getOutOfWayDirection *= (otherAICheckRadius / clampedMagnitude - 1f) * repelFromOthersForce;
            
            repelClampedMagnitude =  clampedMagnitude; //debug
            repelMagnitude =  getOutOfWayDirection.magnitude; //debug
            repelFromOthersVector = getOutOfWayDirection;
            targetPosition = target.position;
            obstacleUpwardsVector = target.up;
            
            return;
        }
        
        repelFromOthersVector = Vector3.zero;
        targetPosition = target.position;
        obstacleUpwardsVector = target.up;
    }
}
*/