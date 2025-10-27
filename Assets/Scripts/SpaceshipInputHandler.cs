using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipInputHandler : MonoBehaviour
{
    private InputSystem_Actions inputSystemActions;
    
    public float pitchDelta {get; private set;}
    public float yawDelta {get; private set;}
    public float rollDelta {get; private set;}
    public float forwardMovement {get; private set;}

    private void Awake()
    {
        inputSystemActions = new InputSystem_Actions();
        inputSystemActions.Enable();
        inputSystemActions.Player.Enable();

        inputSystemActions.Player.Look.performed += OnMouseMove;
        inputSystemActions.Player.Look.canceled += OnMouseMove;
        inputSystemActions.Player.Move.performed += OnMove;
        inputSystemActions.Player.Move.canceled += OnMove;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveVec =  context.ReadValue<Vector2>();
        
        rollDelta = moveVec.x;
        forwardMovement = -moveVec.y;
    }

    private void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 lookVec = context.ReadValue<Vector2>();
        
        yawDelta = lookVec.x;
        pitchDelta = lookVec.y;
    }
}
