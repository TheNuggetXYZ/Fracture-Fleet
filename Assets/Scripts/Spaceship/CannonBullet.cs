using System;
using UnityEngine;

public class CannonBullet : MonoBehaviour
{
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
        float targetDistance = Vector3.Distance(transform.position, targetPosition);
        
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, targetDistance, hitMask);
        if (hit.transform is not null && hit.transform != owner&& (hit.transform.parent is null || hit.transform.parent != owner))
        {
            targetPosition = hit.point;
            ReturnObjectToPool();
        }
        
        transform.position = targetPosition;
    }

    private void ReturnObjectToPool()
    {
        if (returnedObjectToPool) return;
        
        returnedObjectToPool = true;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    //public bool CompareOwner(Transform other) => owner.Equals(other);
}
