using System;
using UnityEngine;

[RequireComponent(typeof(SpaceshipInputHandler), typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private SpaceshipInputHandler input;
    private Rigidbody rb;
    
    [SerializeField] private WorldMenu worldMenu;
    [SerializeField] private float torqueForce = 0.3f;
    [SerializeField] private float rollTorqueForce = 2;
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    [SerializeField] private float movementSpeed = 20;
    [SerializeField] private float verticalMovementSpeed = 10;
    [SerializeField] private float linearDrag = 1;
    [SerializeField] private bool counteractRigidbodyMass = true;
    
    [Header("Debug")]
    [SerializeField] private float velocity;
    
    public float speedFactor {get; private set;}

    private void Awake()
    {
        input = GetComponent<SpaceshipInputHandler>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        velocity = rb.linearVelocity.magnitude;
        speedFactor = Mathf.InverseLerp(0, movementSpeed, velocity); // stationary - 0, max speed - 1

        float somethingCloseToMaxSpeed = movementSpeed * rollSpeedBoostMultiplier + verticalMovementSpeed;
        worldMenu.ShowCriticalSpeedWarning(rb.linearVelocity.magnitude > somethingCloseToMaxSpeed - 5);
    }

    private void FixedUpdate()
    {
        float forceMultiplier = counteractRigidbodyMass ? rb.mass : 1;
        
        rb.AddTorque(transform.right * (input.pitchDelta * torqueForce * forceMultiplier), ForceMode.Force);
        rb.AddTorque(transform.up * (input.yawDelta * torqueForce * forceMultiplier), ForceMode.Force);
        rb.AddTorque(transform.forward * (input.rollDelta * rollTorqueForce * forceMultiplier), ForceMode.Force);

        float speedMultiplier = input.rollDelta != 0 ? rollSpeedBoostMultiplier : 1;
        
        rb.AddForce(transform.forward * (input.forwardMovement * movementSpeed * forceMultiplier * speedMultiplier), ForceMode.Force);
        rb.AddForce(transform.up * (input.verticalMovement * verticalMovementSpeed * forceMultiplier), ForceMode.Force);
    }
}
