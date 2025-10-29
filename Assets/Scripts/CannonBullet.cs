using System;
using UnityEngine;

public class CannonBullet : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float despawnDistanceFromSender = 2000;
    [SerializeField] private Rigidbody rb;
    
    private float actualSpeed;
    
    private Transform owner;

    public void Initialize(Transform sender, float inheritedSpeed)
    {
        owner = sender;
        actualSpeed = inheritedSpeed + speed;
    }

    private void Update()
    {
        if (owner is null)
            return;
        
        transform.position += transform.forward * (actualSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, owner.position) >= despawnDistanceFromSender)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != owner && (other.transform.parent is null || other.transform.parent != owner))
        {
            Destroy(gameObject);
        }
    }

    public bool CompareOwner(Transform other) => owner.Equals(other);
}
