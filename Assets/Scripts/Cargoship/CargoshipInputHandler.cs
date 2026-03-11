using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CargoshipInputHandler : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    
    public float forwardMovement {get; private set;}
    public float pitchDelta {get; private set;}
    public float yawDelta {get; private set;}
    public float rollDelta {get; private set;}

    private void Awake()
    {
        inputActions = GameManager.I.input;
    }

    private void OnEnable()
    {
        inputActions.Cargoship.Move.performed += OnMove;
        inputActions.Cargoship.Move.canceled += OnMove;
        inputActions.Cargoship.Turn.performed += OnTurn;
        inputActions.Cargoship.Turn.canceled += OnTurn;
    }

    private void OnMove(InputAction.CallbackContext cc)
    {
        if (cc.canceled)
        {
            forwardMovement = 0;
            rollDelta = 0;
        }
        else
        {
            forwardMovement = cc.ReadValue<Vector2>().y;
            rollDelta = cc.ReadValue<Vector2>().x;
        }
    }
    
    private void OnTurn(InputAction.CallbackContext cc)
    {
        yawDelta = cc.ReadValue<Vector2>().x;
        pitchDelta = -cc.ReadValue<Vector2>().y;
    }
}
