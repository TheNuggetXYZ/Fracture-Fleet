using System;
using UnityEngine;

public class AIBrain : MonoBehaviour, IShootInput
{
    [Header("General")]
    [field: SerializeField]
    public Rigidbody rb { get; private set; }

    [SerializeField] private Transform target;
    [SerializeField] private bool findPlayer = true;

    [Header("Obstacles")] [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float obstacleClearance = 7f;
    [SerializeField] private float obstacleAvoidAheadMultiplier = 0.25f;

    [Header("Avoid others")] [SerializeField]
    private LayerMask otherAIMask;

    [SerializeField] private float otherAICheckRadius = 30f;
    [SerializeField] private float repelFromOthersForce = 1f;
    [SerializeField] private float repelLerpSpeed = 3f;

    [Header("Output (readonly)")] public Action AfterBrainUpdate;
    public Quaternion targetRotation { get; private set; }

    private Vector3 previousRepelVector;
    private bool shouldShoot;

    private void Update()
    {
        DetectObstacles(out var targetPosition, out var upVector, out var repelVector);
        CalculateRotation(targetPosition, upVector, repelVector);
        AfterBrainUpdate?.Invoke();
    }

    private void CalculateRotation(Vector3 targetPosition, Vector3 upVector, Vector3 repelVector)
    {
        Vector3 targetDirection = targetPosition - transform.position;

        Vector3 combined = (targetDirection.normalized + Vector3.ClampMagnitude(repelVector, 1)).normalized;

        if (combined == Vector3.zero)
            combined = transform.forward;

        targetRotation = Quaternion.LookRotation(combined, upVector);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 3 * Time.deltaTime);
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

            targetPosition = hit.transform.position + obstacleCenterDirNormalized * (obstacleCenterDirMagnitude +
                (hit.distance * obstacleAvoidAheadMultiplier) + obstacleClearance);

            upVector = obstacleCenterDir.normalized;
            repelVector = Vector3.Lerp(previousRepelVector, Vector3.zero, repelLerpSpeed * Time.deltaTime);
            previousRepelVector = repelVector;
            shouldShoot = false;
            return;
        }
        else
        {
            // check player line of sight
            shouldShoot = true;
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

        targetPosition = target.position;
        upVector = target.up;

        repelVector = Vector3.Lerp(previousRepelVector, repelVector, repelLerpSpeed * Time.deltaTime);
        previousRepelVector = repelVector;
    }

    public bool IsShooting() => shouldShoot;
}