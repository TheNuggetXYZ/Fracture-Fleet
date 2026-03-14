using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineBasicMultiChannelPerlin noiseModule;
    [SerializeField] private float noiseAmplitudeAtMaxSpeed;
    [SerializeField] private float noiseFrequencyAtMaxSpeed;
    
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Vector2 FOVRange;
    [FormerlySerializedAs("WarpFOV")]
    [SerializeField] private int SpecialFOV;

    [SerializeField] private SpaceshipController target;
    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }
    
    private void Update()
    {
        noiseModule.AmplitudeGain = noiseAmplitudeAtMaxSpeed * target.normalSpeedFactor;
        noiseModule.FrequencyGain = noiseFrequencyAtMaxSpeed * target.normalSpeedFactor;
        
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(FOVRange.x, FOVRange.y, target.normalSpeedFactor);
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(FOVRange.x, SpecialFOV, target.fullSpeedFactor);
    }
}
