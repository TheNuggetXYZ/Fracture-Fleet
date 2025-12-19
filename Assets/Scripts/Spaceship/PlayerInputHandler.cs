using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, IShootInput
{
    private InputSystem_Actions inputActions;
    
    public float pitchDelta {get; private set;}
    public float yawDelta {get; private set;}
    public float rollDelta {get; private set;}
    public float forwardMovement {get; private set;}
    public float verticalMovement {get; private set;} 
    public bool isShooting {get; private set;}
    public bool isWarping {get; private set;}
    public Action onWarp;

    private void Start()
    {
        inputActions = GameManager.I.input;
        
        inputActions.Player.Look.performed += OnMouseMove;
        inputActions.Player.Look.canceled += OnMouseMove;
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.VerticalMovement.performed += OnVerticalMovement;
        inputActions.Player.VerticalMovement.canceled += OnVerticalMovement;
        inputActions.Player.Attack.performed += OnShoot;
        inputActions.Player.Attack.canceled += OnShoot;
        inputActions.Player.Jump.performed += OnWarp;
        inputActions.Player.Jump.canceled += OnWarp;
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
