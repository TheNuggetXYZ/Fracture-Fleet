using Unity.Cinemachine;
using UnityEngine;

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

    public void SwitchToCam(Camera camera)
    {
        if (camera.camera.Priority.Enabled)
            lastPriority = camera.camera.Priority.Value = lastPriority + 1;
    }
}
