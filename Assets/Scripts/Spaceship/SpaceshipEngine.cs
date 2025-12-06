using System;
using UnityEngine;

public class SpaceshipEngine : SpaceshipPart
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform thrusterTexture;
    [SerializeField] private Transform thrusterEffect;

    private float originalVolume;

    private void Awake()
    {
        originalVolume = audioSource.volume;
    }

    public void SetVolume(float originalVolumeMultiplier)
    {
        audioSource.volume = originalVolume * originalVolumeMultiplier;
    }
    
    public void TurnOff()
    {
        thrusterTexture.gameObject.SetActive(false);
        thrusterEffect.gameObject.SetActive(false);
        audioSource.enabled = false;
    }
}
