using System;
using UnityEngine;

[RequireComponent(typeof(IShootInput))]
public class SpaceshipCannon : MonoBehaviour
{
    private IShootInput input;

    [SerializeField] private CannonBullet bulletPrefab;
    [SerializeField] private Transform[] gunPoints;
    [SerializeField] private float shootCooldown;
    
    private Utils.Timer shootTimer;

    private Vector3 lastPos;
    private float cannonVelocity;
    
    private void Awake()
    {
        input = GetComponent<IShootInput>();

        shootTimer = new Utils.Timer(shootCooldown);
    }

    private void Update()
    {
        shootTimer.Decrement();
        
        if (input.IsShooting())
        {
            if (shootTimer.IsDone())
            {
                Shoot();
                
                shootTimer.Reset(shootCooldown);
            }
        }
    }

    private void LateUpdate()
    {
        cannonVelocity = (transform.position - lastPos).magnitude / Time.deltaTime;
        lastPos = transform.position;
    }

    private void Shoot()
    {
        foreach (Transform gunPoint in gunPoints)
        {
            ObjectPoolManager.SpawnObject(bulletPrefab, transform, cannonVelocity, gunPoint.position, gunPoint.rotation);
        }
    }
}
