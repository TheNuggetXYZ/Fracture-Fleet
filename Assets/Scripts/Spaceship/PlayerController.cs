using System;
using UnityEngine;

[RequireComponent(typeof(SpaceshipInputHandler), typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private SpaceshipInputHandler input;
    private Rigidbody rb;
    
    [SerializeField] private WorldMenu worldMenu;
    [SerializeField] private float torqueForce;
    [SerializeField] private float rollTorqueForce;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float verticalMovementSpeed;
    [SerializeField] private float linearDrag;
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

        worldMenu.ShowCriticalSpeedWarning(rb.linearVelocity.magnitude > movementSpeed + verticalMovementSpeed - 5);
    }

    private void FixedUpdate()
    {
        float forceMultiplier = 1;
        if (counteractRigidbodyMass)
            forceMultiplier = rb.mass;
        
        rb.AddTorque(transform.right * (input.pitchDelta * torqueForce * forceMultiplier), ForceMode.Force);
        rb.AddTorque(transform.up * (input.yawDelta * torqueForce * forceMultiplier), ForceMode.Force);
        rb.AddTorque(transform.forward * (input.rollDelta * rollTorqueForce * forceMultiplier), ForceMode.Force);
        
        rb.AddForce(transform.forward * (input.forwardMovement * movementSpeed * forceMultiplier), ForceMode.Force);
        rb.AddForce(transform.up * (input.verticalMovement * verticalMovementSpeed * forceMultiplier), ForceMode.Force);
    }
}
