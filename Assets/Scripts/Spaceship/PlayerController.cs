using System;
using UnityEngine;

[RequireComponent(typeof(SpaceshipInputHandler), typeof(Rigidbody))]
public class PlayerController : SpaceshipController
{
    private SpaceshipInputHandler input;
    
    [SerializeField] private WorldMenu worldMenu;
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    /*[SerializeField] private WorldMenu worldMenu;
    [SerializeField] private float torqueForce = 0.3f;
    [SerializeField] private float rollTorqueForce = 2;
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    [SerializeField] private float movementSpeed = 20;
    [SerializeField] private float verticalMovementSpeed = 10;
    [SerializeField] private float linearDrag = 1;
    [SerializeField] private bool counteractRigidbodyMass = true;*/
    
    [Header("Debug")]
    [SerializeField] private float velocity;
    
    public float speedFactor {get; private set;}

    private new void Awake()
    {
        base.Awake();
        input = GetComponent<SpaceshipInputHandler>();
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
