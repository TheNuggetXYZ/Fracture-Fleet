using System;
using UnityEngine;

public class PlayerScrapGrabber : MonoBehaviour
{
    private PlayerInputHandler input;
    
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabRadius;
    [SerializeField] private float stabilizeRadius;
    [SerializeField] private Vector2 grabForceRange;
    [SerializeField] private LayerMask scrapMask;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        if (input.isAttractingScrap)
            AttractParts();
    }

    private void AttractParts()
    {
        Collider[] hits = Physics.OverlapSphere(grabPoint.position, grabRadius, scrapMask);

        foreach (var hit in hits)
        {
            float distance = Vector3.Distance(hit.transform.position, grabPoint.position);
            Vector3 inDirection = grabPoint.position - hit.transform.position;
            
            if (distance <= stabilizeRadius)
            {
                hit.attachedRigidbody.linearVelocity = rb.linearVelocity;
            }
            else
            {
                float grabForceIntensity = Mathf.InverseLerp(0, grabRadius, distance);
                
                hit.attachedRigidbody.AddForce(inDirection * Mathf.Lerp(grabForceRange.x, grabForceRange.y, grabForceIntensity), ForceMode.Acceleration);
            }
        }
    }
}
