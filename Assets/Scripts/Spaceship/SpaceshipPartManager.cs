using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SpaceshipPartManager : MonoBehaviour, ITakeDamage
{
    [SerializeField] private SpaceshipController spaceshipController;
    
    [Header("Ship Condition")]
    [SerializeField] private int shipHealth;
    [SerializeField] private int lostHealthOnPartKill;
    [SerializeField] private float onDeathExplosionForce;
    [SerializeField] private float onDeathExplosionSize = 1;
    
    [Header("Collisions")]
    [SerializeField] private float collisionMagnitudeThreshold;
    
    [Header("Parts")]
    [SerializeField] private bool fetchChildParts;
    [SerializeField] private Transform childPartsParent;
    [FormerlySerializedAs("killableParts")] [SerializeField] private SpaceshipPart[] allParts;
    
    private bool shipDead;
    private int maxShipHealth;
    private List<SpaceshipEngine> engines = new();
    private List<Transform> metalSparkEffectList = new();
    
    private Rigidbody spaceshipRigidbody;

    private void OnValidate()
    {
        if (fetchChildParts)
        {
            childPartsParent ??= transform;
            
            allParts = new SpaceshipPart[childPartsParent.childCount];
            
            for (int i = 0; i < childPartsParent.childCount; i++)
            {
                Transform child = childPartsParent.GetChild(i);

                if (child && child.gameObject.activeInHierarchy)
                {
                    SpaceshipPart childPart = child.GetComponent<SpaceshipPart>();
                    if(childPart && childPart.enabled)
                    {
                        allParts[i] = childPart;

                        var engine = allParts[i] as SpaceshipEngine;
                        if (engine)
                            engines.Add(engine);
                    }
                }
            }
        }
    }

    private void Awake()
    {
        spaceshipRigidbody = GetComponent<Rigidbody>();
        maxShipHealth = shipHealth;
    }

    private void Update()
    {
        foreach (SpaceshipEngine engine in engines)
        {
            engine.SetVolume(spaceshipController.speedFactor);
        }
    }

    public void TakeDamage(int damage, Transform hitCollider, Vector3 hitVelocity = default)
    {
        ShipTakeDamage(damage);
        if (shipDead) return;
        
        foreach (SpaceshipPart part in allParts)
        {
            // if we can hurt the part and if it's the right one
            if (part && !part.IsUnkillable && part.PartCollider.transform == hitCollider)
            {
                part.TakeDamage(damage);
                
                // if part should die
                if (part.PartHealth <= 0)
                {
                    // kill part
                    part.Kill(GetVelocity() + hitVelocity, false, out bool successfullyKilled);
                    
                    // damage ship and apply modifiers (usually debuffs)
                    if (successfullyKilled)
                    {
                        ShipTakeDamage(lostHealthOnPartKill);
                        if (shipDead) return;
                        
                        part.GetOnKillModifiers(out SpaceshipPart.OnKillModifierType[] types, out float[] values);
                        ApplyOnKillModifiers(types, values);
                    }
                }
            }
        }
    }

    private void ShipTakeDamage(int damage)
    {
        shipHealth -= damage;

        if (shipHealth <= 0 && !shipDead)
        {
            KillShip();
        }
    }

    private void KillShip()
    {
        shipDead = true;
        spaceshipController.KillShip();
        GetComponent<AIBrain>()?.ShipDied();
        SpawnScrap();
        TurnOffEngines();
        
        ObjectPoolManager.SpawnObject(GameManager.I.prefabs.shipDeathExplosionVFX, transform.position, default, onDeathExplosionSize * Vector3.one);
        
        gameObject.SetActive(false);
    }

    private void SpawnScrap()
    {
        foreach (SpaceshipPart part in allParts)
        {
            if (part)
            {
                Vector3 explosionVelocity = (part.transform.position - transform.position).normalized * onDeathExplosionForce;
                part.Kill(GetVelocity() + explosionVelocity, true, out bool _);
            }
        }
    }

    private void TurnOffEngines()
    {
        foreach (SpaceshipEngine engine in engines)
        {
            engine.TurnOff();
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
            metalSparkEffectList.Add(Instantiate(GameManager.I.prefabs.metalSparkVFX, cp.point, Quaternion.LookRotation(-cp.normal), cp.thisCollider.transform));
        }
    }

    public SpaceshipPart[] GetKilledParts() => allParts.Where(part => part.IsKilled).ToArray();

    public void ClearAllModifiers()
    {
        spaceshipController.ClearModifiers();
    }

    public void Heal()
    {
        shipHealth = maxShipHealth;
    }

    public void RemoveSparks()
    {
        foreach (Transform effect in metalSparkEffectList)
        {
            Destroy(effect.gameObject);
        }
        
        metalSparkEffectList.Clear();
    }
}
