using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceObject : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    
    [field: SerializeField] public float mass {get; private set;}
    [SerializeField] private float radius;
    [SerializeField] private float densityKgPerMeter;
    [SerializeField] private bool calculateMass;
    [field: SerializeField] public Vector3 initialVelocity {get; private set;}
    [field: SerializeField] public bool lockedPosition {get; private set;}
    
    private Vector3 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        currentVelocity = initialVelocity;
    }
    
    public void UpdateVelocity(SpaceObject otherObject)
    {
        if (lockedPosition)
            return;
        
        Vector3 forceDirection = (otherObject.transform.position - transform.position).normalized;
        float objectDistanceSqr = (otherObject.transform.position - transform.position).sqrMagnitude;
        float G = SpaceGravitySimulator.Instance.gravitationalConstant;
        Vector3 force = forceDirection * (G * (mass * otherObject.mass / objectDistanceSqr));
        Vector3 acceleration = force / mass;

        currentVelocity += acceleration * Time.fixedDeltaTime;
    }

    public void UpdatePosition()
    {
        rb.MovePosition(transform.position + currentVelocity * Time.fixedDeltaTime);
    }

    private void OnValidate()
    {
        transform.localScale = Vector3.one * radius * 2;
        
        if (calculateMass)
        {
            // sphere mass
            float massNoDensity = 2.3561945f * Mathf.Pow(radius, 3); // 2.35... is 3/4 * pi
            mass = massNoDensity * densityKgPerMeter * SpaceGravitySimulator.densityMultiplier;
            rb.mass = mass;
        }
    }
}
