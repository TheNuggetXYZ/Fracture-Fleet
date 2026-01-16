using System;
using UnityEngine;

public class FloatingParticleSystem : MonoBehaviour
{
    [SerializeField] private new ParticleSystem particleSystem;
    [SerializeField] private bool overrideLocalSimulationSpace;

    GameManager game;
    
    private void Start()
    {
        game = GameManager.I;
        
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        if (overrideLocalSimulationSpace || particleSystem.main.simulationSpace != ParticleSystemSimulationSpace.Local)
        {
            Override();
        }
    }

    private void Override()
    {
        var main = particleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Custom;
        main.customSimulationSpace = game.floatingOrigin.originalOrigin;
    }
}
