using System;
using Unity.Cinemachine;
using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    [SerializeField] private Transform[] movables;
    [SerializeField] private CinemachineCamera[] cameraMovables;

    private Vector3 positionOffset;
    
    private void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
    }

    private void OnCameraUpdated(CinemachineBrain brain)
    {
    }

    private void LateUpdate()
    {
        positionOffset = -transform.position;
        
        // Shift world
        MoveObjects(movables, positionOffset);
        MoveObjects(ObjectPoolManager.GetPoolParents(), positionOffset); // ERRORS AFTER DYING AND SCENE RELOAD
        MoveObjects(cameraMovables, positionOffset);
        
        // Move player back
        transform.position = Vector3.zero;
    }

    private void MoveObjects(Transform[] objects, Vector3 move)
    {
        foreach (var obj in objects)
            obj.position += move;
    }

    private void MoveObjects(CinemachineCamera[] cameras, Vector3 move)
    {
        foreach (var cam in cameras)
        {
            cam.OnTargetObjectWarped(transform, move);
        }
        /*foreach (var cam in cameras)
            cam.ForceCameraPosition(cam.transform.position + move, cam.transform.rotation);*/
    }
}