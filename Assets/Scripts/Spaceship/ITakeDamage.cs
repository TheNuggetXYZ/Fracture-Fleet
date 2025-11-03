using UnityEngine;

public interface ITakeDamage
{
    public void TakeDamage(int damage, Transform hitCollider, Vector3 hitVelocity = default);
}
