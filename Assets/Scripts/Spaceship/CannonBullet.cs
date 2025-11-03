using System;
using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float despawnDistanceFromSender = 2000;
    [SerializeField] private LayerMask hitMask;

    private float actualSpeed;

    private Transform owner;

    private bool returnedObjectToPool = false;

    public void Initialize(Transform sender, float inheritedSpeed)
    {
        returnedObjectToPool = false;
        owner = sender;
        actualSpeed = inheritedSpeed + speed;
    }

    private void Update()
    {
        if (owner is null)
            return;

        if (Vector3.Distance(transform.position, owner.position) >= despawnDistanceFromSender)
            ReturnObjectToPool();

        Vector3 targetPosition = transform.position + transform.forward * (actualSpeed * Time.deltaTime);

        DetectCollisions(ref targetPosition);

        transform.position = targetPosition;
    }

    private void DetectCollisions(ref Vector3 targetPosition)
    {
        float targetDistance = Vector3.Distance(transform.position, targetPosition);

        // Ray from current to target position to check if we will collide
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, targetDistance, hitMask);
        
        if (hit.transform && !IsOwner(hit.transform))
        {
            hit.transform.GetComponent<ITakeDamage>()?.TakeDamage(damage, hit.collider.transform);

            targetPosition = hit.point;
            ReturnObjectToPool();
        }
    }

    private void ReturnObjectToPool()
    {
        if (returnedObjectToPool) return;

        returnedObjectToPool = true;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private bool IsOwner(Transform other)
    {
        Transform lastTransform = other;
        while (true)
        {
            if (lastTransform == owner)
                return true;
            
            if (!lastTransform)
                return false;
            
            lastTransform = lastTransform.parent;
        }
    }

//public bool CompareOwner(Transform other) => owner.Equals(other);
}
