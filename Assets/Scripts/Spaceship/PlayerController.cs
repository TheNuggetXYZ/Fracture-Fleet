using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(Rigidbody))]
public class PlayerController : SpaceshipController
{
    private PlayerInputHandler input;
    private SpaceshipGravity gravity;
    
    [SerializeField] private float rollSpeedBoostMultiplier = 1.3f;
    [SerializeField] private float warpSpeedBoostMultiplier = 5f;
    [SerializeField] private float warpCancelCollisionMagnitudeThreshold = 10;
    [SerializeField] private float warpChargeRotationSpeedMultiplier = 0.2f;
    [SerializeField] private AudioSource warpChargeSFX;
    
    public float warpSpeedFactor {get; private set;}

    private float currentWarpSpeedMultiplier;
    private bool canWarp;
    private bool warpCharging;
    
    GameManager game;

    private new void Awake()
    {
        base.Awake();
        game = GameManager.I;
        input = GetComponent<PlayerInputHandler>();
        gravity = GetComponent<SpaceshipGravity>();

    }

    private void OnEnable()
    {
        input.onWarp += WarpStart;
    }

    private void OnDisable()
    {
        input.onWarp -= WarpStart;
    }

    private new void Update()
    {
        base.Update();
        
        warpSpeedFactor = Mathf.InverseLerp(0, MovementSpeed * warpSpeedBoostMultiplier, velocity);
        
        game.popupListHandler.ShowPopup(game.popupListHandler.popup_Warping, IsWarping() && !warpCharging);
        game.popupListHandler.ShowPopup(game.popupListHandler.warning_HighGravity, gravity.totalGravity.magnitude > MovementSpeed);

        if (!input.isWarping && warpRoutine && warpCoroutine != null)
        {
            StopCoroutine(warpCoroutine);
            warpChargeSFX.Stop();
            game.popupListHandler.ShowPopup(game.popupListHandler.popup_ChargingWarp, false);
            warpCharging = warpRoutine = false;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
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
    }

    private void HandleRotation()
    {
        float rotationMultiplier = warpCharging ? warpChargeRotationSpeedMultiplier : 1;
        
        if (!IsWarping() || warpCharging)
            Rotate(transform.right * input.pitchDelta, 
                transform.up * input.yawDelta, 
                transform.forward * input.rollDelta,
                rotationMultiplier);
    }

    private void WarpStart()
    {
        if (game.gamePaused)
            return;
        
        canWarp = true;
        
        if (!warpRoutine)
            warpCoroutine = StartCoroutine(WarpRoutine());
    }

    private bool warpRoutine;
    private Coroutine warpCoroutine;
    private IEnumerator WarpRoutine()
    {
        warpRoutine = warpCharging = true;
        game.popupListHandler.ShowPopup(game.popupListHandler.popup_ChargingWarp, true);
        
        warpChargeSFX.Play();
        currentWarpSpeedMultiplier = 0;
        yield return new WaitForSeconds(warpChargeSFX.clip.length - 2.415f); // specific to the current clip
        currentWarpSpeedMultiplier = warpSpeedBoostMultiplier;
        
        game.popupListHandler.ShowPopup(game.popupListHandler.popup_ChargingWarp, false);
        warpRoutine = warpCharging = false;
    }

    private bool IsWarping() => input.isWarping && canWarp;

    private void OnCollisionEnter(Collision collision)
    {
        float collisionMagnitude = collision.relativeVelocity.magnitude;

        if (!warpCharging && collisionMagnitude >= warpCancelCollisionMagnitudeThreshold)
            canWarp = false;
    }
}
