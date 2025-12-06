using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(Rigidbody))]
public class PlayerController : SpaceshipController
{
    private PlayerInputHandler input;
    
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    [SerializeField] private float warpSpeedBoostMultiplier = 5f;
    [SerializeField] private float warpCancelCollisionMagnitudeThreshold = 10;
    
    [Header("Debug")]
    [SerializeField] private float velocity;
    
    public float speedFactor {get; private set;}
    public float warpSpeedFactor {get; private set;}

    private float currentWarpSpeedMultiplier;
    private bool canWarp;

    private new void Awake()
    {
        base.Awake();
        input = GetComponent<PlayerInputHandler>();

        input.onWarp += WarpStart;
    }

    private void Update()
    {
        velocity = rb.linearVelocity.magnitude;
        speedFactor = Mathf.InverseLerp(0, MovementSpeed, velocity); // stationary - 0, max speed - 1
        warpSpeedFactor = Mathf.InverseLerp(0, MovementSpeed * warpSpeedBoostMultiplier, velocity);
        
        //ManageCriticalSpeedWarning();

        if (!input.isWarping && warpRoutine)
            StopCoroutine(WarpRoutine());
    }

    private void FixedUpdate()
    {
        float forwardMovementMultiplier = 1;
        if (IsWarping())
            forwardMovementMultiplier = currentWarpSpeedMultiplier;
        else if (input.rollDelta != 0)
            forwardMovementMultiplier = rollSpeedBoostMultiplier;

        // move only forward during warping
        float forwardMovement = IsWarping() ? 1 : input.forwardMovement;
        float verticalMovement = IsWarping() ? 0 : input.verticalMovement;

        Move(MovementSpeed, 
            forwardMovement, 
            verticalMovement, 
            forwardMovementMultiplier);
        
        if (!IsWarping())
            Rotate(transform.right * input.pitchDelta, 
                transform.up * input.yawDelta, 
                transform.forward * input.rollDelta);
    }

    private void WarpStart()
    {
        canWarp = true;
        
        if (!warpRoutine)
            StartCoroutine(WarpRoutine());
    }

    private bool warpRoutine;
    private IEnumerator WarpRoutine()
    {
        warpRoutine = true;
        
        currentWarpSpeedMultiplier = 0;
        yield return new WaitForSeconds(3);
        currentWarpSpeedMultiplier = warpSpeedBoostMultiplier;
        
        warpRoutine = false;
    }

    private bool IsWarping() => input.isWarping && canWarp;

    private void OnCollisionEnter(Collision collision)
    {
        float collisionMagnitude = collision.relativeVelocity.magnitude;

        if (collisionMagnitude >= warpCancelCollisionMagnitudeThreshold)
            canWarp = false;
    }

    private void ManageCriticalSpeedWarning()
    {
        float somethingCloseToMaxSpeed = MovementSpeed * rollSpeedBoostMultiplier + VerticalMovementSpeed;
        GameManager.I.worldMenu.ShowObject(GameManager.I.worldMenu.criticalSpeedWarning,  
            !input.isWarping && rb.linearVelocity.magnitude > somethingCloseToMaxSpeed - 5);
    }
}
