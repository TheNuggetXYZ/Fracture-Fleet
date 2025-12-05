using System;
using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float despawnDistanceFromSender = 2000;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Transform hitEffect;

    private Vector3 inheritedVelocity;

    private Transform owner;

    private bool returnedObjectToPool = false;

    public void Initialize(Transform sender, Vector3 inheritedVelocity)
    {
        returnedObjectToPool = false;
        owner = sender;
        this.inheritedVelocity = inheritedVelocity;
    }

    private void Update()
    {
        if (owner is null)
            return;

        if (Vector3.Distance(transform.position, owner.position) >= despawnDistanceFromSender)
            ReturnObjectToPool();

        Vector3 targetPosition = transform.position + transform.forward * (speed * Time.deltaTime) + inheritedVelocity * Time.deltaTime;

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
            hit.transform.GetComponent<ITakeDamage>()?.TakeDamage(damage, hit.collider.transform, targetPosition - transform.position);

            SpawnEffect(hit.point, hit.normal, hit.collider.transform);
            
            targetPosition = hit.point;
            ReturnObjectToPool();
        }
    }

    private void SpawnEffect(Vector3 position, Vector3 direction, Transform parent)
    {
        position += direction.normalized * 0.1f;
        ObjectPoolManager.SpawnObject(hitEffect.gameObject, position, Quaternion.LookRotation(direction), null, parent);
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
