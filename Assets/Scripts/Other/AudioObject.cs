using System;
using UnityEngine;

public class AudioObject : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool randomizeVolume;
    [SerializeField] private float randomizeVolumeSpeed;
    
    public AudioSource AudioSource => audioSource;
    
    private float originalVolume = -1f;
    private float originalPitch = -1f;

    private void Awake()
    {
        if (originalVolume == -1f)
        {
            originalVolume = audioSource.volume;
        }

        if (originalPitch == -1f)
        {
            originalPitch = audioSource.pitch;
        }
    }

    private void Update()
    {
        if (randomizeVolume)
        {
            audioSource.volume = originalVolume * Mathf.Sin(Time.time * randomizeVolumeSpeed);
        }
    }

    public void SetVolumeMultiplier(float multiplier)
    {
        audioSource.volume = originalVolume * multiplier;
    }
    
    public void SetPitchMultiplier(float multiplier)
    {
        audioSource.pitch = originalPitch * multiplier;
    }
    
    public void AddPitch(float add)
    {
        audioSource.pitch = originalPitch + add;
    }
}
