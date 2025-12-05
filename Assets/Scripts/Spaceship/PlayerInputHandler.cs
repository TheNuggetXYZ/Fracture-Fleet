using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, IShootInput
{
    private InputSystem_Actions inputSystemActions;
    
    public float pitchDelta {get; private set;}
    public float yawDelta {get; private set;}
    public float rollDelta {get; private set;}
    public float forwardMovement {get; private set;}
    public float verticalMovement {get; private set;} 
    public bool isShooting {get; private set;}
    public bool isWarping {get; private set;}
    public Action onWarp;

    private void Awake()
    {
        inputSystemActions = new InputSystem_Actions();
        inputSystemActions.Enable();
        inputSystemActions.Player.Enable();

        inputSystemActions.Player.Look.performed += OnMouseMove;
        inputSystemActions.Player.Look.canceled += OnMouseMove;
        inputSystemActions.Player.Move.performed += OnMove;
        inputSystemActions.Player.Move.canceled += OnMove;
        inputSystemActions.Player.VerticalMovement.performed += OnVerticalMovement;
        inputSystemActions.Player.VerticalMovement.canceled += OnVerticalMovement;
        inputSystemActions.Player.Attack.performed += OnShoot;
        inputSystemActions.Player.Attack.canceled += OnShoot;
        inputSystemActions.Player.Jump.performed += OnWarp;
        inputSystemActions.Player.Jump.canceled += OnWarp;
    }

    private void OnWarp(InputAction.CallbackContext context)
    {
        isWarping = !context.canceled;
        
        if (context.performed)
            onWarp?.Invoke();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        isShooting = !context.canceled;
    }

    private void OnVerticalMovement(InputAction.CallbackContext context)
    {
        float vertical =  context.ReadValue<float>();
        
        verticalMovement = vertical;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveVec =  context.ReadValue<Vector2>();
        
        rollDelta = -moveVec.x;
        forwardMovement = moveVec.y;
    }

    private void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 lookVec = context.ReadValue<Vector2>();
        
        yawDelta = lookVec.x;
        pitchDelta = -lookVec.y;
    }

    public bool IsShooting() => isShooting;
}
