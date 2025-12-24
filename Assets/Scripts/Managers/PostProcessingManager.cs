using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] private Volume volume;
    
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    private float originalChromaticAberrationIntensity;
    private float originalLensDistortionIntensity;
    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
        
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out lensDistortion);

        if (chromaticAberration)
            originalChromaticAberrationIntensity = chromaticAberration.intensity.value;
        
        if (lensDistortion)
            originalLensDistortionIntensity = lensDistortion.intensity.value;
    }

    private void OnEnable()
    {
        game.player.OnWarpStart += SetWarpVisuals;
        game.player.OnWarpEnd += UnsetWarpVisuals;
    }

    private void OnDisable()
    {
        game.player.OnWarpStart -= SetWarpVisuals;
        game.player.OnWarpEnd -= UnsetWarpVisuals;
    }

    private void SetWarpVisuals()
    {
        chromaticAberration.intensity.Override(0.5f);
        lensDistortion.intensity.Override(-0.5f);
    }
    
    private void UnsetWarpVisuals()
    {
        chromaticAberration.intensity.Override(originalChromaticAberrationIntensity);
        lensDistortion.intensity.Override(originalLensDistortionIntensity);
    }
}
