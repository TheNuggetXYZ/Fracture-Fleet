using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpaceshipInputHandler), typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private SpaceshipInputHandler input;
    private Rigidbody rb;
    
    [SerializeField] private float torqueForce;
    [SerializeField] private float rollTorqueForce;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float verticalMovementSpeed;
    [SerializeField] private bool counteractRigidbodyMass = true;
    
    [Header("Debug")]
    [SerializeField] private float velocity;
    
    public float speedFactor {get; private set;}

    private void Awake()
    {
        input = GetComponent<SpaceshipInputHandler>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        velocity = rb.linearVelocity.magnitude;
        speedFactor = Mathf.InverseLerp(0, movementSpeed, velocity); // stationary - 0, max speed - 1
        
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
