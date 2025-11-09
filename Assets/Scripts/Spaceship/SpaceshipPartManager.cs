using System;
using UnityEngine;

public class SpaceshipPartManager : MonoBehaviour, ITakeDamage
{
    [SerializeField] private SpaceshipController spaceshipController;
    
    [Header("Ship Condition")]
    [SerializeField] private int shipHealth;
    [SerializeField] private int lostHealthOnPartKill;
    [SerializeField] private float onDeathExplosionForce;
    [SerializeField] private Transform deathExplosionEffect;
    
    [Header("Collisions")]
    [SerializeField] private float collisionMagnitudeThreshold;
    [SerializeField] private Transform metalSparkEffect;
    
    [Header("Parts")]
    [SerializeField] private bool fetchChildParts;
    [SerializeField] private Transform childPartsParent;
    [SerializeField] private SpaceshipPart[] killableParts;
    
    private bool isDead;
    
    private Rigidbody spaceshipRigidbody;

    private void OnValidate()
    {
        if (fetchChildParts)
        {
            childPartsParent ??= transform;
            
            killableParts = new SpaceshipPart[childPartsParent.childCount];
            
            for (int i = 0; i < childPartsParent.childCount; i++)
            {
                Transform child = childPartsParent.GetChild(i);

                if (child && child.gameObject.activeInHierarchy)
                {
                    SpaceshipPart childPart = child.GetComponent<SpaceshipPart>();
                    if(childPart && childPart.enabled)
                        killableParts[i] = childPart;
                }
            }
        }
    }

    private void Awake()
    {
        spaceshipRigidbody = GetComponent<Rigidbody>();
    }
    
    public void TakeDamage(int damage, Transform hitCollider, Vector3 hitVelocity = default)
    {
        shipHealth -= damage;
        if (CheckIfShipIsDead()) return;
        
        foreach (SpaceshipPart part in killableParts)
        {
            if (part && !part.IsUnkillable && part.PartCollider.transform == hitCollider)
            {
                part.TakeDamage(damage);
                
                if (part.PartHealth <= 0)
                {
                    part.Kill(GetVelocity() + hitVelocity, false, out bool successfullyKilled);
                    
                    if (successfullyKilled)
                    {
                        shipHealth -= lostHealthOnPartKill;
                        
                        if (CheckIfShipIsDead()) return;
                        
                        part.GetOnKillModifiers(out SpaceshipPart.OnKillModifierType[] types, out float[] values);
                        ApplyOnKillModifiers(types, values);
                    }
                }
            }
        }
    }

    private bool CheckIfShipIsDead()
    {
        if (shipHealth <= 0 && !isDead)
        {
            isDead = true;
            ObjectPoolManager.SpawnObject(deathExplosionEffect.gameObject, transform.position);
            spaceshipController.KillShip();
            GetComponent<AIBrain>()?.ShipDied();
            SpawnScrap();
            return true;
        }

        return false;
    }

    private void SpawnScrap()
    {
        foreach (SpaceshipPart part in killableParts)
        {
            if (part)
            {
                Vector3 explosionVelocity = (part.transform.position - transform.position).normalized * onDeathExplosionForce;
                part.Kill(GetVelocity() + explosionVelocity, true, out bool _);
            }
        }
    }

    private void ApplyOnKillModifiers(SpaceshipPart.OnKillModifierType[] types, float[] values)
    {
        if (!spaceshipController) return;
        
        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case SpaceshipPart.OnKillModifierType.rotation:
                    spaceshipController.AddRotationModifier(values[i]);
                    break;
                
                case SpaceshipPart.OnKillModifierType.speed:
                    spaceshipController.AddMovementModifier(values[i]);
                    break;
                
                case SpaceshipPart.OnKillModifierType.unstableRotation:
                    spaceshipController.AddUnstableRotationModifier(values[i]);
                    break;
            }
        }
    }

    private Vector3 GetVelocity() => spaceshipRigidbody ? spaceshipRigidbody.linearVelocity : Vector3.zero;

    private void OnCollisionEnter(Collision collision)
    {
        float collisionMagnitude = collision.relativeVelocity.magnitude;

        if (collisionMagnitude >= collisionMagnitudeThreshold)
        {
            ContactPoint cp = collision.GetContact(0);
            Instantiate(metalSparkEffect, cp.point, Quaternion.LookRotation(-cp.normal), transform);
        }
    }
}
