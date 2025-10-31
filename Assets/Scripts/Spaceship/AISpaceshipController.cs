using UnityEngine;

public class AISpaceshipController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private AIBrain brain;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float zoomSpeed = 30f;
    [SerializeField] private float rotationLerpSpeed = 3f;
    [SerializeField] private bool counteractRigidbodyMass = true;

    [Header("Debug")] 
    public bool move = true;

    private void Start()
    {
        brain.AfterBrainUpdate += AfterBrainUpdate;
    }

    private void AfterBrainUpdate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, brain.targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (move)
        {
            float forceMultiplier = counteractRigidbodyMass ? brain.rb.mass : 1f;
            float actualSpeed = brain.currentState == AIBrain.AIState.zoomingPast ? zoomSpeed : speed;
            brain.rb.AddForce(transform.forward * (actualSpeed * forceMultiplier), ForceMode.Force);
        }
    }
}