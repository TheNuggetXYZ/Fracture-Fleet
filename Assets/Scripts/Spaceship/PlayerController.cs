using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(Rigidbody))]
public class PlayerController : SpaceshipController
{
    private PlayerInputHandler input;
    
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    [SerializeField] private float warpSpeedBoostMultiplier = 5f;
    
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
        
        
    }

    private void FixedUpdate()
    {
        float forwardMovementMultiplier = 1;
        if (input.isWarping)
            forwardMovementMultiplier = warpSpeedBoostMultiplier;
        else if (input.rollDelta != 0)
            forwardMovementMultiplier = rollSpeedBoostMultiplier;

        // move only forward during warping
        float forwardMovement = input.isWarping ? 1 : input.forwardMovement;
        float verticalMovement = input.isWarping ? 0 : input.verticalMovement;

        Move(MovementSpeed, 
            forwardMovement, 
            verticalMovement, 
            forwardMovementMultiplier);
        
        if (!input.isWarping)
            Rotate(transform.right * input.pitchDelta, 
                transform.up * input.yawDelta, 
                transform.forward * input.rollDelta);
    }

    private void ManageCriticalSpeedWarning()
    {
        float somethingCloseToMaxSpeed = MovementSpeed * rollSpeedBoostMultiplier + VerticalMovementSpeed;
        GameManager.I.worldMenu.ShowObject(GameManager.I.worldMenu.criticalSpeedWarning,  
            !input.isWarping && rb.linearVelocity.magnitude > somethingCloseToMaxSpeed - 5);
    }
}
