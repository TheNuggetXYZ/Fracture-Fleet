using System;
using UnityEngine;

public class CargoshipController : SpaceshipController
{
    [SerializeField] private float rotationSpeed;
    
    GameManager game;
    
    private CargoshipInputHandler inputHandler;

    private new void Awake()
    {
        base.Awake();
        
        game = GameManager.I;
        
        inputHandler = GetComponent<CargoshipInputHandler>();
    }

    private void FixedUpdate()
    {
        Move(MovementSpeed, inputHandler.forwardMovement);
        
        Rotate(transform.right * inputHandler.pitchDelta, 
            transform.up * inputHandler.yawDelta, 
            transform.forward * inputHandler.rollDelta, 
            rotationSpeed);
    }
}
