using System;
using UnityEngine;

public class AudioOPMReturner : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Invoke(nameof(ReturnObject), audioSource.clip.length);
    }

    private void ReturnObject()
    {
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}
