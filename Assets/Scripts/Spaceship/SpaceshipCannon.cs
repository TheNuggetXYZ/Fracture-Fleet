using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpaceshipCannon : MonoBehaviour
{
    private IShootInput input;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private CannonBullet bulletPrefab;
    [SerializeField] private Transform[] gunPoints;
    [SerializeField] private float shootCooldown;
    
    private Utils.Timer shootTimer;
    private Vector3 averageGunPointPosition;
    
    private void Awake()
    {
        input = GetComponent<IShootInput>();

        shootTimer = new Utils.Timer(shootCooldown);

        Vector3 gunPointPosSum = Vector3.zero;
        foreach (var gunPoint in gunPoints)
        {
            gunPointPosSum += gunPoint.localPosition;
        }

        averageGunPointPosition = gunPointPosSum / gunPoints.Length;
    }

    private void Update()
    {
        shootTimer.Decrement();
        
        if (!Utils.IsNull(input) && !EventSystem.current.IsPointerOverGameObject() && !GameManager.I.gamePaused && input.IsShooting())
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
        
        ObjectPoolManager.SpawnObject(GameManager.I.prefabs.shipShootSFX, transform.TransformPoint(averageGunPointPosition));

        foreach (Transform gunPoint in gunPoints)
        {
            ObjectPoolManager.SpawnObject(bulletPrefab, transform, rb.linearVelocity, gunPoint.position, gunPoint.rotation);
        }
    }
}
