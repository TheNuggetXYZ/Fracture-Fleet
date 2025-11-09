using UnityEngine;

public class AISpaceshipController : SpaceshipController
{
    [Header("Stats")]
    [SerializeField] private AIBrain brain;
    [SerializeField] private float zoomSpeed = 30f;

    [Header("Debug")] 
    public bool move = true;

    private void Start()
    {
        brain.AfterBrainUpdate += AfterBrainUpdate;
    }

    private void AfterBrainUpdate()
    {
        Quaternion delta = brain.targetRotation * Quaternion.Inverse(transform.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        Vector3 torque = axis * (angle * Mathf.Deg2Rad);
        Rotate(torque);
    }

    private void FixedUpdate()
    {
        if (move)
        {
            float actualSpeed = brain.currentState == AIBrain.AIState.zoomingPast ? zoomSpeed : MovementSpeed;
            Move(actualSpeed, 1, 0);
        }
    }
}