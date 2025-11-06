using System;
using UnityEngine;

public class SunEngulfing : MonoBehaviour
{
    [SerializeField] private float drag;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody)
        {
            other.attachedRigidbody.linearVelocity *= 1f / (1f + drag * Time.fixedDeltaTime);
        }
    }
}
