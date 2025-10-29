using System;
using UnityEngine;

[RequireComponent(typeof(SpaceshipInputHandler))]
public class SpaceshipCannon : MonoBehaviour
{
    private IShootInput input;

    [SerializeField] private CannonBullet bulletPrefab;
    [SerializeField] private Transform gunPoint;
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
        if (input.IsShooting())
        {
            shootTimer.Decrement();
            if (shootTimer.IsDone())
            {
                Shoot();
                
                shootTimer.Reset();
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
        Instantiate(bulletPrefab, gunPoint.position, gunPoint.rotation).Initialize(transform, cannonVelocity);
    }
}
