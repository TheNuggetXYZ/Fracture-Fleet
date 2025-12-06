using System;
using UnityEngine;

public class PSForceFieldController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ParticleSystemForceField particleSystemForceField;
    [SerializeField] private Vector2 forceFieldStrengthRange;
    [SerializeField] private Vector2 forceFieldSizeRange;

    private void Update()
    {
        ParticleSystem.MinMaxCurve newStrengthValue = particleSystemForceField.gravity;
        newStrengthValue.constant = Mathf.Lerp(forceFieldStrengthRange.x, forceFieldStrengthRange.y, playerController.warpSpeedFactor);
        particleSystemForceField.gravity = newStrengthValue;
        
        particleSystemForceField.endRange = Mathf.Lerp(forceFieldSizeRange.x, forceFieldSizeRange.y, playerController.warpSpeedFactor);
    }
}
