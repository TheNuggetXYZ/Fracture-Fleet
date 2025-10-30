using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceObject : MonoBehaviour
{
    private Rigidbody rb;
    
    [field: SerializeField] public float mass {get; private set;}
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
}
