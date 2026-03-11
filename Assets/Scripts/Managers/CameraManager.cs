using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    private int lastPriority = 0;
    
    [System.Serializable]
    public class Camera
    {
        public CinemachineCamera camera;
        public CameraController cameraController;
    }

    [field: SerializeField] public Camera CamPlayerCockpit {get; private set;}
    [field: SerializeField] public Camera CamCargoship {get; private set;}

    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }

    private void OnEnable()
    {
        game.input.Player.ToggleToCargoship.performed += ToggleCargoshipCam;
    }
    
    private void OnDisable()
    {
        game.input.Player.ToggleToCargoship.performed -= ToggleCargoshipCam;
    }

    private void ToggleCargoshipCam(InputAction.CallbackContext cc)
    {
        if (!CamCargoship.camera.IsLive) 
            SwitchToCam(CamCargoship);
        else
            SwitchToCam(CamPlayerCockpit);
    }
    
    public void SwitchToCam(Camera camera)
    {
        if (camera.camera.Priority.Enabled)
            lastPriority = camera.camera.Priority.Value = lastPriority + 1;
        else
            Debug.Log("Priority disabled on cam: " + camera.camera);
    }
}
