using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CrosshairSmartPlacer : MonoBehaviour
{
    [SerializeField] private Transform crosshair;
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Transform cameraViewPoint;
    [SerializeField] private float distanceFromCamera;

    private void OnValidate()
    {
        Vector3 directionFromCamToTarget = aimTarget.position - cameraViewPoint.position;
        directionFromCamToTarget.Normalize();
        
        crosshair.position = cameraViewPoint.position + Utils.ResizeVector(directionFromCamToTarget, distanceFromCamera);
    }
}
