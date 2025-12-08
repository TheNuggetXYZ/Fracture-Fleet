using System;
using UnityEngine;

public class SpaceshipGravity : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float gravityConstantMultiplier = 2f;

    [SerializeField] public bool move = true;

    public void ApplyGravity(SpaceObject otherObject)
    {
        if (!move)
            return;
        
        Vector3 forceDirection = (otherObject.transform.position - transform.position).normalized;
        float objectDistanceSqr = (otherObject.transform.position - transform.position).sqrMagnitude;
        float G = SpaceGravitySimulator.Instance.gravitationalConstant * gravityConstantMultiplier;
        Vector3 acceleration = forceDirection * (G * (otherObject.mass / objectDistanceSqr));

        rb.AddForce(acceleration, ForceMode.Acceleration);
    }
}
