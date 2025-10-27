using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpaceshipInputHandler), typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour
{
    private SpaceshipInputHandler input;
    private Rigidbody rb;
    
    [SerializeField] private float torqueForce;
    [SerializeField] private float rollTorqueForceMultiplier;
    [SerializeField] private float movementSpeed;

    private void Awake()
    {
        input = GetComponent<SpaceshipInputHandler>();
        rb = GetComponent<Rigidbody>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        rb.AddTorque(transform.right * (input.pitchDelta * torqueForce), ForceMode.Force);
        rb.AddTorque(transform.up * (input.yawDelta * torqueForce), ForceMode.Force);
        rb.AddTorque(transform.forward * (input.rollDelta * rollTorqueForceMultiplier * torqueForce), ForceMode.Force);
        
        rb.AddForce(transform.forward * (input.forwardMovement * movementSpeed), ForceMode.Force);
    }
}
