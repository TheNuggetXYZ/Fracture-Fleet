using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineBasicMultiChannelPerlin noiseModule;
    [SerializeField] private float noiseAmplitudeAtMaxSpeed;
    [SerializeField] private float noiseFrequencyAtMaxSpeed;
    
    [SerializeField] private CinemachineCamera camera;
    [SerializeField] private Vector2 FOVRange;

    private PlayerController player;
    
    private void Start()
    {
        player = GameManager.I.player;
    }
    
    private void Update()
    {
        noiseModule.AmplitudeGain = noiseAmplitudeAtMaxSpeed * player.speedFactor;
        noiseModule.FrequencyGain = noiseFrequencyAtMaxSpeed * player.speedFactor;
        
        camera.Lens.FieldOfView = Mathf.Lerp(FOVRange.x, FOVRange.y, player.speedFactor);
    }
}
