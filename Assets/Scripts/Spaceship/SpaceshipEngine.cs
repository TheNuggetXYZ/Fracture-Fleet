using System;
using UnityEngine;

public class SpaceshipEngine : MonoBehaviour
{
    [SerializeField] private SpaceshipController controller;
    [SerializeField] private Transform thrusterTexture;
    [SerializeField] private Transform thrusterEffect;

    private void Update()
    {
        thrusterTexture.gameObject.SetActive(controller.enabled);
        thrusterEffect.gameObject.SetActive(controller.enabled);
    }
}
