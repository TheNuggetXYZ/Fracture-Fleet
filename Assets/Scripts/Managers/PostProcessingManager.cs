using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] private Volume volume;

    [Header("Warp visuals")]
    [SerializeField] private float warpChromaticAberrationIntensity = .5f;
    [SerializeField] private float warpLensDistortionIntensity = -.5f;
    [SerializeField] private float warpVignetteIntensity = .5f;
    private float originalChromaticAberrationIntensity;
    private float originalLensDistortionIntensity;
    private float originalVignetteIntensity;
    
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Vignette vignette;

    
    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
        
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out vignette);

        originalChromaticAberrationIntensity = chromaticAberration.intensity.value;
        originalLensDistortionIntensity = lensDistortion.intensity.value;
        originalVignetteIntensity = vignette.intensity.value;
    }

    private void Update()
    {
        SetWarpVisuals(game.player.warpSpeedFactor);
    }

    private void SetWarpVisuals(float lerp)
    {
        chromaticAberration.intensity.Override(Mathf.Lerp(originalChromaticAberrationIntensity, warpChromaticAberrationIntensity, lerp));
        lensDistortion.intensity.Override(Mathf.Lerp(originalLensDistortionIntensity, warpLensDistortionIntensity, lerp));
        vignette.intensity.Override(Mathf.Lerp(originalVignetteIntensity, warpVignetteIntensity, lerp));
    }
}
