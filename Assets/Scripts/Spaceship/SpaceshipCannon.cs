using System;
using UnityEngine;

public class SpaceshipCannon : MonoBehaviour
{
    private IShootInput input;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private CannonBullet bulletPrefab;
    [SerializeField] private Transform[] gunPoints;
    [SerializeField] private float shootCooldown;
    
    private Utils.Timer shootTimer;
    
    private void Awake()
    {
        input = GetComponent<IShootInput>();

        shootTimer = new Utils.Timer(shootCooldown);
    }

    private void Update()
    {
        shootTimer.Decrement();
        
        if (!Utils.IsNull(input) && input.IsShooting())
        {
            if (shootTimer.IsDone())
            {
                Shoot();
                
                shootTimer.Reset(shootCooldown);
            }
        }
    }

    private void Shoot()
    {
        foreach (Transform gunPoint in gunPoints)
        {
            ObjectPoolManager.SpawnObject(bulletPrefab, transform, rb.linearVelocity, gunPoint.position, gunPoint.rotation);
        }
    }
}
