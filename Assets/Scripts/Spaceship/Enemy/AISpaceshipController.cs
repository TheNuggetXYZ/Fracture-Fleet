using UnityEngine;

public class AISpaceshipController : SpaceshipController
{
    [Header("Stats")]
    [SerializeField] private AIBrain brain;
    [SerializeField] private float zoomSpeed = 30f;

    [Header("Debug")] 
    public bool canMove = true;

    private void FixedUpdate()
    {
        HandleRotation();
        
        if (canMove)
            HandleMovement();
    }

    private void HandleRotation()
    {
        Quaternion delta = brain.targetRotation * Quaternion.Inverse(transform.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        Vector3 torque = axis * (angle * Mathf.Deg2Rad);
        Rotate(torque);
    }

    private void HandleMovement()
    {
        float actualSpeed = 0;
        if (brain.speedTier == 1)
            actualSpeed = MovementSpeed;
        else if (brain.speedTier == 2)
            actualSpeed = zoomSpeed;

        if (actualSpeed != 0)
            Move(actualSpeed, 1, 0);
    }
}