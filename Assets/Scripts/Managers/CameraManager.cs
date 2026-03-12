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

    /// <returns> success? </returns>
    public bool SwitchToCam(Camera camera)
    {
        if (camera.camera.Priority.Enabled)
        {
            lastPriority = camera.camera.Priority.Value = lastPriority + 1;
            return true;
        }
        
        Debug.Log("Priority disabled on cam: " + camera.camera);
        return false;
    }
    
    /// <returns> state of the toggled camera </returns>
    public bool ToggleCargoshipCam(InputAction.CallbackContext cc)
    {
        if (!CamCargoship.camera.IsLive)
            return SwitchToCam(CamCargoship);
        
        return !SwitchToCam(CamPlayerCockpit);
    }
}
