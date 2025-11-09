using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(Rigidbody))]
public class PlayerController : SpaceshipController
{
    private PlayerInputHandler input;
    
    [SerializeField] private WorldMenu worldMenu;
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    
    [Header("Debug")]
    [SerializeField] private float velocity;
    
    public float speedFactor {get; private set;}

    private new void Awake()
    {
        base.Awake();
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        velocity = rb.linearVelocity.magnitude;
        speedFactor = Mathf.InverseLerp(0, MovementSpeed, velocity); // stationary - 0, max speed - 1
        
        float somethingCloseToMaxSpeed = MovementSpeed * rollSpeedBoostMultiplier + VerticalMovementSpeed;
        worldMenu.ShowCriticalSpeedWarning(rb.linearVelocity.magnitude > somethingCloseToMaxSpeed - 5);
    }

    private void FixedUpdate()
    {
        Rotate(transform.right * input.pitchDelta, transform.up * input.yawDelta, transform.forward * input.rollDelta);

        Move(MovementSpeed, input.forwardMovement, input.verticalMovement, input.rollDelta != 0 ? rollSpeedBoostMultiplier : 1);
    }
}
