using System;
using UnityEngine;

public class SpaceshipGravity : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float gravityConstantMultiplier = 20f;

    [SerializeField] public bool move = true;
    public Vector3 totalGravity;

    public void ApplyGravity(SpaceObject otherObject)
    {
        if (!move)
            return;
        
        Vector3 forceDirection = (otherObject.transform.position - transform.position).normalized;
        float objectDistanceSqr = (otherObject.transform.position - transform.position).sqrMagnitude;
        float G = SpaceGravitySimulator.Instance.gravitationalConstant * gravityConstantMultiplier;
        Vector3 acceleration = forceDirection * (G * (otherObject.mass / objectDistanceSqr));

        // totalGravity needs to be set to 0 before applying spaceship gravity with all space objects
        totalGravity += acceleration;
        
        rb.AddForce(acceleration, ForceMode.Acceleration);
    }
}
