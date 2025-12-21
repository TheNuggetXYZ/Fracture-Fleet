using UnityEngine;

public class ParticleSystemMover : MonoBehaviour
{
    [SerializeField] private Transform parent;
    [SerializeField] private float moveAheadBasedOnParentSpeedMultiplier;

    Vector3 lastParentPos;

    private void FixedUpdate()
    {
        Vector3 changeOfParentPos = parent.position - lastParentPos;
        transform.position = parent.position + changeOfParentPos * moveAheadBasedOnParentSpeedMultiplier;
        lastParentPos = parent.position;
    }
}
