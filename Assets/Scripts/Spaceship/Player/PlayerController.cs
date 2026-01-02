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

    private State state;
    private float currentWarpSpeedMultiplier;

    public Action OnWarpStart;
    public Action OnWarpEnd;
    
    GameManager game;
    SaveManager save;
    
    private enum State
    {
        Normal = 0,
        WarpCharging = 1,
        Warp = 2,
    }

    private new void Awake()
    {
        base.Awake();
        game = GameManager.I;
        save = SaveManager.I;
        input = GetComponent<PlayerInputHandler>();
        gravity = GetComponent<SpaceshipGravity>();
        state = State.Normal;
    }

    private void OnEnable()
    {
        input.onWarp += WarpStart;
    }

    private void OnDisable()
    {
        input.onWarp -= WarpStart;
        
        StopWarp();
    }

    private new void Update()
    {
        base.Update();
        
        warpSpeedFactor = Mathf.InverseLerp(0, MovementSpeed * warpSpeedBoostMultiplier, velocity);
        
        game.popupListHandler.ShowPopup(game.popupListHandler.popup_Warping, state == State.Warp);
        game.popupListHandler.ShowPopup(game.popupListHandler.warning_HighGravity, gravity.totalGravity.magnitude > MovementSpeed);

        if (!input.isWarping)
            StopWarp();
    }

    private void StopWarp()
    {
        state = State.Normal;
        
        OnWarpEnd?.Invoke();
        
        // try stop warp charging
        if (warpCoroutine != null)
        {
            StopCoroutine(warpCoroutine);
            warpChargeSFX.Stop();
            game.popupListHandler.ShowPopup(game.popupListHandler.popup_ChargingWarp, false);
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        
        if (state != State.Warp)
            HandleRotation();
    }

    private void HandleMovement()
    {
        float forwardMovementMultiplier = 1;
        if (state == State.WarpCharging || state == State.Warp)
            forwardMovementMultiplier = currentWarpSpeedMultiplier;
        else if (input.rollDelta != 0)
            forwardMovementMultiplier = rollSpeedBoostMultiplier;

        // move only forward during warping
        float forwardMovement = state == State.Warp ? 1 : input.forwardMovement;
        float verticalMovement = state == State.Warp ? 0 : input.verticalMovement;

        Move(MovementSpeed, 
            forwardMovement, 
            verticalMovement, 
            forwardMovementMultiplier);
    }

    private void HandleRotation()
    {
        float rotationMultiplier = state == State.WarpCharging ? warpChargeRotationSpeedMultiplier : 1;

        Rotate(transform.right * (input.pitchDelta * save.saveData.sensitivity),
            transform.up * (input.yawDelta * save.saveData.sensitivity),
            transform.forward * input.rollDelta,
            rotationMultiplier);
    }

    private void WarpStart()
    {
        if (!game.gamePaused && state != State.WarpCharging)
            warpCoroutine = StartCoroutine(WarpRoutine());
    }

    private Coroutine warpCoroutine;
    private IEnumerator WarpRoutine()
    {
        state = State.WarpCharging;
        game.popupListHandler.ShowPopup(game.popupListHandler.popup_ChargingWarp, true);
        
        warpChargeSFX.Play();
        currentWarpSpeedMultiplier = 0;
        yield return new WaitForSeconds(warpChargeSFX.clip.length - 2.415f); // specific to the current clip
        currentWarpSpeedMultiplier = warpSpeedBoostMultiplier;
        
        game.popupListHandler.ShowPopup(game.popupListHandler.popup_ChargingWarp, false);
        state = State.Warp;
        
        OnWarpStart?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float collisionMagnitude = collision.relativeVelocity.magnitude;

        if (state != State.WarpCharging && collisionMagnitude >= warpCancelCollisionMagnitudeThreshold)
        {
            StopWarp();
        }
    }
}
