using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SpaceshipPartManager : MonoBehaviour, ITakeDamage
{
    [field: SerializeField] public SpaceshipController spaceshipController {get; private set;}
    
    [Header("Ship Condition")]
    [SerializeField] private bool playerShip;
    [SerializeField] private int shipHealth;
    [SerializeField] private int lostHealthOnPartKill;
    [SerializeField] private float onDeathExplosionForce;
    [SerializeField] private float onDeathExplosionSize = 1;
    
    [Header("Collisions")]
    [SerializeField] private float sparksCollisionMagnitudeThreshold = 15;
    [SerializeField] private float lightHitCollisionMagnitudeThreshold = 5;
    [SerializeField] private float mediumHitCollisionMagnitudeThreshold = 15;
    [SerializeField] private float heavyHitCollisionMagnitudeThreshold = 30;
    
    [Header("Parts")]
    [SerializeField] private bool fetchChildParts;
    [SerializeField] private Transform childPartsParent;
    [FormerlySerializedAs("killableParts")] [SerializeField] private SpaceshipPart[] allParts;
    [SerializeField] private bool debug_killAppParts;
    
    private bool shipDead;
    private int maxShipHealth;
    private List<SpaceshipEngine> engines = new();
    private List<Transform> metalSparkEffectList = new();
    private AudioObject metalSparkSFX;
    
    private Rigidbody spaceshipRigidbody;
    private GameManager game;

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
        game = GameManager.I;
        spaceshipRigidbody = GetComponent<Rigidbody>();
        maxShipHealth = shipHealth;

        if (engines.Count == 0 || engines[0] == null)
        {
            foreach (SpaceshipPart part in allParts)
            {
                var engine = part as SpaceshipEngine;
                if (engine)
                    engines.Add(engine);
            }
        }
    }

    private void Update()
    {
        if (playerShip)
            game.popupListHandler.ShowPopup(game.popupListHandler.warning_DestructionImminent, shipHealth <= lostHealthOnPartKill);
        
        foreach (SpaceshipEngine engine in engines)
        {
            //TODO: not only boost the volume for player warping, but for AI zooming since they have a zooming speed!!!
            engine.SetVolume(spaceshipController.speedFactor);
        }

        if (debug_killAppParts)
        {
            foreach (SpaceshipPart part in allParts)
            {
                if (part && !part.IsUnkillable)
                {
                    Vector3 explosionVelocity = (part.transform.position - transform.position).normalized * onDeathExplosionForce;
                    part.Kill(GetVelocity() + explosionVelocity, false, out bool _);
                }
            }

            debug_killAppParts = false;
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

                    if (playerShip)
                        game.popupListHandler.ShowPopup(game.popupListHandler.warning_ShipModuleLost, true, 0, 2);
                    
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

        if (!playerShip)
        {
            game.popupListHandler.ShowPopup(game.popupListHandler.popup_EnemyNeutralized, true, 0.5f, 2);
            game.waveManager.EnemyDefeated();
        }
        
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
        ContactPoint cp = collision.GetContact(0);
        
        if (collisionMagnitude >= sparksCollisionMagnitudeThreshold)
        {
            metalSparkEffectList.Add(Instantiate(GameManager.I.prefabs.metalSparkVFX, cp.point, Quaternion.LookRotation(-cp.normal), cp.thisCollider.transform));
            
            if (playerShip)
                game.popupListHandler.ShowPopup(game.popupListHandler.popup_ShipSustainedDamage, true, 0, 2);

            if (metalSparkEffectList.Count == 1 && !metalSparkSFX)
                metalSparkSFX = ObjectPoolManager.SpawnObject(GameManager.I.prefabs.metalSparkSFX, cp.point, 1, 0, false, default, null, transform);
        }
        
        if (collisionMagnitude >= heavyHitCollisionMagnitudeThreshold)
        {
            float volumeMult = (collisionMagnitude / heavyHitCollisionMagnitudeThreshold) * 0.15f;
            ObjectPoolManager.SpawnObject(GameManager.I.prefabs.heavyHitSFXp1, cp.point, volumeMult);
            ObjectPoolManager.SpawnObject(GameManager.I.prefabs.heavyHitSFXp2, cp.point, volumeMult);
            
            //TODO: damage the ship, maybe kill a random part, or damage all parts by 1
            
            if (playerShip)
                game.popupListHandler.ShowPopup(game.popupListHandler.warning_ImpactDamage, true, 0, 2);
        }
        else if (collisionMagnitude >= mediumHitCollisionMagnitudeThreshold)
        {
            float volumeMult = (collisionMagnitude / mediumHitCollisionMagnitudeThreshold) * 0.3f;
            ObjectPoolManager.SpawnObject(GameManager.I.prefabs.mediumHitSFX, cp.point, volumeMult);
        }
        else if (collisionMagnitude >= lightHitCollisionMagnitudeThreshold)
        {
            float volumeMult = (collisionMagnitude / lightHitCollisionMagnitudeThreshold) * 0.3f;
            ObjectPoolManager.SpawnObject(GameManager.I.prefabs.lightHitSFX, cp.point, volumeMult);
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
        
        if (playerShip)
            game.popupListHandler.ShowPopup(game.popupListHandler.popup_IntegrityRestored, true, 0, 2);
    }

    public void RemoveSparks()
    {
        foreach (Transform effect in metalSparkEffectList)
        {
            Destroy(effect.gameObject);
        }
        
        metalSparkEffectList.Clear();
        
        if (metalSparkSFX)
            ObjectPoolManager.ReturnObjectToPool(metalSparkSFX.gameObject);
        metalSparkSFX = null;
    }
}
