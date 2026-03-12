using UnityEngine;
using UnityEngine.InputSystem;

public class StateManager : MonoBehaviour
{
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }

    private void OnEnable()
    {
        game.input.World.ToggleToCargoship.performed += ToggleToCargoship;
    }
    
    private void OnDisable()
    {
        game.input.World.ToggleToCargoship.performed -= ToggleToCargoship;
    }

    private void ToggleToCargoship(InputAction.CallbackContext context)
    {
        bool isCamLive = game.cameraManager.ToggleCargoshipCam(default);

        if (isCamLive)
        {
            game.input.Player.Disable();
            game.input.Cargoship.Enable();
        }
        else
        {
            game.input.Player.Enable();
            game.input.Cargoship.Disable();
        }
        
    }
}
