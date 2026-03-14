using System;
using UnityEngine;

public class PSForceFieldController : MonoBehaviour
{
    [SerializeField] private SpaceshipController spaceshipController;
    [SerializeField] private ParticleSystemForceField particleSystemForceField;
    [SerializeField] private Vector2 forceFieldStrengthRange;
    [SerializeField] private Vector2 forceFieldSizeRange;

    private void Update()
    {
        ParticleSystem.MinMaxCurve newStrengthValue = particleSystemForceField.gravity;
        newStrengthValue.constant = Mathf.Lerp(forceFieldStrengthRange.x, forceFieldStrengthRange.y, spaceshipController.fullSpeedFactor);
        particleSystemForceField.gravity = newStrengthValue;
        
        particleSystemForceField.endRange = Mathf.Lerp(forceFieldSizeRange.x, forceFieldSizeRange.y, spaceshipController.fullSpeedFactor);
    }
}
