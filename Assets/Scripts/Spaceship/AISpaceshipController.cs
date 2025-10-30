using UnityEngine;

public class AISpaceshipController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private AIBrain brain;
    [SerializeField] private float speed = 15f;
    [SerializeField] private float rotationLerpSpeed = 3f;

    [Header("Debug")] 
    public bool move = true;

    private void Start()
    {
        brain.AfterBrainUpdate += AfterBrainUpdate;
    }

    private void AfterBrainUpdate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, brain.targetRotation, rotationLerpSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (move)
            brain.rb.AddForce(transform.forward * speed, ForceMode.Force);
    }
}