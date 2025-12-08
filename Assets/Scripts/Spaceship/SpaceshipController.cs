using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpaceshipController : MonoBehaviour
{
    protected Rigidbody rb;
    protected SpaceshipGravity sGravity;
    
    [SerializeField] private float movementSpeed = 20;
    [SerializeField] private float torqueForce = 0.3f;
    [SerializeField] private float rollTorqueForce = 2;
    [SerializeField] private float verticalMovementSpeed = 10;
    [SerializeField] protected bool counteractRigidbodyMass = true;
    [SerializeField] protected float velocity; // debug

    private bool move = true;
    private bool rotate = true;
    private float movementSpeedMultiplier = 1f;
    private float torqueForceMultiplier = 1f;
    private float unstabilityAmount = 0;
    
    public float speedFactor {get; private set;}
    
    protected float MovementSpeed => movementSpeed;
    protected float VerticalMovementSpeed => verticalMovementSpeed;
    
    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sGravity = GetComponent<SpaceshipGravity>();
    }

    protected void Update()
    {
        velocity = rb.linearVelocity.magnitude;
        speedFactor = Mathf.InverseLerp(0, MovementSpeed, velocity); // stationary - 0, max speed - 1
    }

    protected void Move(float forwardMovementSpeed, float forwardMovement = 1, float verticalMovement = 0, float forwardMovementMultiplier = 1)
    {
        if (!move) return;
        
        float forceMultiplier = counteractRigidbodyMass ? rb.mass : 1f;
        
        rb.AddForce(transform.forward * (forwardMovement * forwardMovementSpeed * movementSpeedMultiplier * forceMultiplier * forwardMovementMultiplier), ForceMode.Force);
        rb.AddForce(transform.up * (verticalMovement * verticalMovementSpeed * movementSpeedMultiplier * forceMultiplier), ForceMode.Force);
    }

    protected void Rotate(Vector3 pitch, Vector3 yaw, Vector3 roll, float multiplier = 1)
    {
        pitch *= torqueForce;
        yaw *= torqueForce;
        roll *= rollTorqueForce;
        
        Rotate_Internal((pitch + yaw + roll) * multiplier);
    }

    protected void Rotate(Vector3 torque)
    {
        Rotate_Internal(torque * torqueForce);
    }

    private void Rotate_Internal(Vector3 finalTorque)
    {
        if (!rotate) return;
        
        float forceMultiplier = counteractRigidbodyMass ? rb.mass : 1f;
        float unstability = Random.Range((1-unstabilityAmount), (1+unstabilityAmount));
        rb.AddTorque(finalTorque * (torqueForceMultiplier * unstability * forceMultiplier), ForceMode.Force);
    }

    public void AddRotationModifier(float value)
    {
        torqueForceMultiplier *= value;
    }
    
    public void AddMovementModifier(float value)
    {
        movementSpeedMultiplier *= value;
    }
    
    public void AddUnstableRotationModifier(float value)
    {
        unstabilityAmount += value;
    }

    public void KillShip()
    {
        rb.linearDamping = 0;
        Lock();
    }

    public void ClearModifiers()
    {
        movementSpeedMultiplier = 1;
        torqueForceMultiplier = 1f;
        unstabilityAmount = 0;
    }

    public void Lock()
    {
        move = false;
        rotate = false;
        
        if (sGravity)
            sGravity.move = false;
    }

    public void Unlock()
    {
        move = true;
        rotate = true;
        
        if (sGravity)
            sGravity.move = true;
    }
}
