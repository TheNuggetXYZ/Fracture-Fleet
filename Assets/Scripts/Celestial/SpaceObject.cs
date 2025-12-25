using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceObject : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    
    [field: SerializeField] public float mass {get; private set;}
    [SerializeField] protected float radius;
    [SerializeField] private float densityKgPerMeter;
    [SerializeField] private bool calculateMass;
    
    [field: SerializeField] public Vector3 initialVelocity { get; protected set; }
    [field: SerializeField] public bool lockedPosition {get; private set;}
    [field: SerializeField] public bool isCelestialBody { get; private set; }

    [SerializeField] public float initialRotationDegreesPerSec;
    [SerializeField] public Vector3 upVectorOverride;
    
    
    //private Vector3 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (!lockedPosition)
            rb.AddForce(initialVelocity, ForceMode.VelocityChange);

        if (upVectorOverride == Vector3.zero)
            upVectorOverride = transform.up;
        
        rb.sleepThreshold = 0;
        
        rb.AddTorque(upVectorOverride * initialRotationDegreesPerSec, ForceMode.Acceleration);
    }
    
    public void UpdateVelocity(SpaceObject otherObject)
    {
        if (lockedPosition)
            return;
        
        Vector3 forceDirection = (otherObject.transform.position - transform.position).normalized;
        float objectDistanceSqr = (otherObject.transform.position - transform.position).sqrMagnitude;
        float G = SpaceGravitySimulator.I.gravitationalConstant;
        Vector3 force = forceDirection * (G * (mass * otherObject.mass / objectDistanceSqr));
        Vector3 acceleration = force / mass;

        rb.AddForce(acceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    protected void OnValidate()
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
