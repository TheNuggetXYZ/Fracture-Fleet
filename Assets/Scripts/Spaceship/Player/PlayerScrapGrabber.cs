using System;
using UnityEngine;

public class PlayerScrapGrabber : MonoBehaviour
{
    private PlayerInputHandler input;
    
    [SerializeField] private float grabRadius;
    [SerializeField] private LayerMask scrapMask;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float attractForce;
    [SerializeField] private float stabilizationForce;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        lineRenderer.positionCount = 0;
        
        if (input.isAttractingScrap)
        {
            Collider[] hits = GetParts();
            DrawLasers(hits);
            AttractParts(hits);
        }
    }

    private Collider[] GetParts()
    {
        // cast a sphere ahead of the player
        return Physics.OverlapSphere(grabPoint.position + transform.forward * grabRadius, grabRadius, scrapMask);
    }

    private void AttractParts(Collider[] hits)
    {
        foreach (Collider col in hits)
        {
            if (!col.attachedRigidbody)
                continue;
            
            Vector3 toGrabPointDir = grabPoint.position - col.transform.position;
            Vector3 toFrontOfPlayerDir = Vector3.ProjectOnPlane(targetPoint.position - col.transform.position, transform.forward);
            float force = Mathf.Lerp(-attractForce, attractForce, Vector3.Distance(grabPoint.position, col.transform.position) / grabRadius);
            //col.attachedRigidbody.AddForce(force * toGrabPointDir + stabilizationForce * toTargetPointDir.normalized);
            col.attachedRigidbody.linearVelocity =
                force * toGrabPointDir + stabilizationForce * toFrontOfPlayerDir.normalized;
        }
    }

    private void DrawLasers(Collider[] hits)
    {
        lineRenderer.positionCount = hits.Length * 2 + 1;
        for (int i = 0; i < hits.Length; i++)
        {
            lineRenderer.SetPosition(2*i, hits[i].transform.position);
            lineRenderer.SetPosition(2*i + 1, grabPoint.position);
        }
    }
}

/*public class PlayerScrapGrabber : MonoBehaviour
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

            if (!hit.attachedRigidbody)
                return;

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
*/