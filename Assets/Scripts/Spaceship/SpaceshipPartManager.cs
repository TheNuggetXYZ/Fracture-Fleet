using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SpaceshipPartManager : MonoBehaviour, ITakeDamage
{
    [field: SerializeField] public SpaceshipController spaceshipController {get; private set;}
    
    [Header("Ship Condition")]
    [field: SerializeField] public ShipType shipType {get; private set;} = ShipType.enemy;
    [SerializeField] private int shipHealth;
    [SerializeField] private int lostHealthOnPartKill;
    [SerializeField] private float onDeathExplosionForce;
    [SerializeField] private float onDeathExplosionSize = 1;
    [SerializeField] private GameObject alarmLights;
    [SerializeField] private AudioSource alarmSFX;
    
    [Header("Collisions")]
    [SerializeField] private float sparksCollisionMagnitudeThreshold = 15;
    [SerializeField] private float lightHitCollisionMagnitudeThreshold = 5;
    [SerializeField] private float mediumHitCollisionMagnitudeThreshold = 15;
    [SerializeField] private float heavyHitCollisionMagnitudeThreshold = 30;
    
    [Header("Parts")]
    [SerializeField] private bool fetchChildParts;
    [SerializeField] private Transform childPartsParent;
    [FormerlySerializedAs("killableParts")] [SerializeField] private SpaceshipPart[] allParts;
    [SerializeField] private bool debug_killAllParts;
    [SerializeField] private bool debug_killShip;
    
    public bool shipDead {get; private set;}
    private int maxShipHealth;
    private List<SpaceshipEngine> engines = new();
    private List<Transform> metalSparkEffectList = new();
    private AudioObject metalSparkSFX;
    
    private Rigidbody spaceshipRigidbody;
    
    public enum ShipType
    {
        enemy,
        comrade,
        player,
    }
    
    GameManager game;

    private void OnValidate()
    {
        FetchChildPartsCheck();
    }

    private void FetchChildPartsCheck()
    {
        engines = new();
        
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

            fetchChildParts = false;
        }
    }

    private void Awake()
    {
        game = GameManager.I;
        
        game.spaceshipTracker.ShipSpawned(this);
        
        spaceshipRigidbody = GetComponent<Rigidbody>();
        
        maxShipHealth = shipHealth;

        if (engines == null || engines.Count == 0 || engines[0] == null)
            FindEnginesFromAllParts();
        
        // the ship type can change after awake (SetShipToComrade) so keep that in mind
    }

    private void OnDestroy() => game.spaceshipTracker.ShipDestroyed();

    private void FindEnginesFromAllParts()
    {
        engines = new();
        foreach (SpaceshipPart part in allParts)
        {
            var engine = part as SpaceshipEngine;
            if (engine)
                engines.Add(engine);
        }
    }

    private void Update()
    {
        UpdateEngineVolumes();
        
        if (shipType == ShipType.player)
            DestructionImminentLogic();
        
        Debug_KillAllPartsCheck();

        if (debug_killShip)
            KillShip();
    }

    private void DestructionImminentLogic()
    {
        bool destructionImminent = shipHealth <= lostHealthOnPartKill;
        game.popupListHandler.ShowPopup(game.popupListHandler.warning_DestructionImminent, destructionImminent);
        
        if (destructionImminent)
            StartAlarm();
        else
            StopAlarm();
    }

    public void TakeDamage(int damage, Transform hitCollider, Vector3 hitVelocity = default)
    {
        DamageShip(damage);
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
                    part.Kill(GetSpaceshipVelocity() + hitVelocity, false, out bool successfullyKilled);

                    if (shipType == ShipType.player)
                        game.popupListHandler.ShowPopup(game.popupListHandler.warning_ShipModuleLost, true, 0, 2);
                    
                    // damage ship and apply modifiers (usually debuffs)
                    if (successfullyKilled)
                    {
                        DamageShip(lostHealthOnPartKill);
                        if (shipDead) return;
                        
                        part.GetOnKillModifiers(out SpaceshipPart.OnKillModifierType[] types, out float[] values);
                        ApplyOnKillModifiers(types, values);
                    }
                }
            }
        }
    }

    private void DamageShip(int damage)
    {
        shipHealth -= damage;

        if (shipHealth <= 0 && !shipDead)
        {
            KillShip();
        }
    }

    public void Debug_KillShip(bool spawnScrapNearPlayer)
    {
        Debug.Log("Killed ship with a debug method: " + gameObject);
        KillShip(spawnScrapNearPlayer);
    }

    private void KillShip(bool spawnScrapNearPlayer = false)
    {
        shipDead = true;
        spaceshipController.KillShip();
        GetComponent<AIBrain>()?.ShipDied();
        TurnOffEngines();
        SpawnScrap(spawnScrapNearPlayer);

        if (shipType == ShipType.enemy)
        {
            game.popupListHandler.ShowPopup(game.popupListHandler.popup_EnemyNeutralized, true, 0.5f, 2);
            game.waveManager.EnemyDefeated();
        }
        else if (shipType == ShipType.player)
            game.PlayerDied();
        
        ObjectPoolManager.SpawnObject(game.prefabs.shipDeathExplosionVFX, transform.position, default, onDeathExplosionSize * Vector3.one);
        
        gameObject.SetActive(false);
    }

    private void SpawnScrap(bool spawnScrapNearPlayer = false)
    {
        foreach (SpaceshipPart part in allParts)
        {
            if (part)
            {
                Vector3 explosionVelocity = (part.transform.position - transform.position).normalized * onDeathExplosionForce;
                part.Kill(GetSpaceshipVelocity() + explosionVelocity, true, out bool _);

                if (spawnScrapNearPlayer)
                {
                    part.transform.position += (game.player.transform.position - transform.position) + game.player.transform.forward * 10;
                    
                    if (part.TryGetComponent(out Rigidbody rb))
                        rb.linearVelocity = Vector3.zero;
                }
            }
        }
    }

    private void UpdateEngineVolumes()
    {
        foreach (SpaceshipEngine engine in engines)
        {
            //TODO: not only boost the volume for player warping, but for AI zooming since they have a zooming speed!!!
            engine.SetVolume(spaceshipController.speedFactor);
        }
    }
    
    private void TurnOffEngines()
    {
        foreach (SpaceshipEngine engine in engines) 
            engine.TurnOff();
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
    
    private Vector3 GetSpaceshipVelocity() => spaceshipRigidbody ? spaceshipRigidbody.linearVelocity : Vector3.zero;
    
    private void Debug_KillAllPartsCheck()
    {
        if (debug_killAllParts)
        {
            foreach (SpaceshipPart part in allParts)
            {
                if (part && !part.IsUnkillable)
                {
                    Vector3 explosionVelocity = (part.transform.position - transform.position).normalized * onDeathExplosionForce;
                    part.Kill(GetSpaceshipVelocity() + explosionVelocity, false, out bool _);
                }
            }

            debug_killAllParts = false;
        }
    }

    public void SetShipToComrade()
    {
        shipType = ShipType.comrade;
    }

#region Alarm
    
    private Coroutine alarmCoroutine;
    private IEnumerator AlarmRoutine()
    {
        alarmSFX.Play();
        
        while (true)
        {
            // values based on a particular sound
            yield return new WaitForSeconds(0.071f);
            alarmLights.SetActive(true);
            yield return new WaitForSeconds(0.668f);
            alarmLights.SetActive(false);
            yield return new WaitForSeconds(0.43f);
            alarmLights.SetActive(true);
            yield return new WaitForSeconds(0.668f);
            alarmLights.SetActive(false);
            yield return new WaitForSeconds(0.396f);
        }
    }

    private void StartAlarm()
    {
        if (alarmCoroutine == null)
            alarmCoroutine = StartCoroutine(AlarmRoutine());
    }

    private void StopAlarm()
    {
        if (alarmCoroutine != null)
        {
            StopCoroutine(alarmCoroutine);
            alarmCoroutine = null;
            alarmSFX.Stop();
            alarmLights.SetActive(false);
        }
    }
    
#endregion

#region Collisions

    private void OnCollisionEnter(Collision collision)
    {
        float collisionMagnitude = collision.relativeVelocity.magnitude;
        ContactPoint cp = collision.GetContact(0);
        
        if (collisionMagnitude >= sparksCollisionMagnitudeThreshold)
        {
            metalSparkEffectList.Add(Instantiate(game.prefabs.metalSparkVFX, cp.point, Quaternion.LookRotation(-cp.normal), cp.thisCollider.transform));
            
            if (shipType == ShipType.player)
                game.popupListHandler.ShowPopup(game.popupListHandler.popup_ShipSustainedDamage, true, 0, 2);

            if (metalSparkEffectList.Count == 1 && !metalSparkSFX)
                metalSparkSFX = ObjectPoolManager.SpawnObject(game.prefabs.metalSparkSFX, cp.point, 1, 0, false, default, null, transform);
        }
        
        if (collisionMagnitude >= heavyHitCollisionMagnitudeThreshold)
        {
            float volumeMult = (collisionMagnitude / heavyHitCollisionMagnitudeThreshold) * 0.15f;
            ObjectPoolManager.SpawnObject(game.prefabs.heavyHitSFXp1, cp.point, volumeMult);
            ObjectPoolManager.SpawnObject(game.prefabs.heavyHitSFXp2, cp.point, volumeMult);
            
            //TODO: damage the ship, maybe kill a random part, or damage all parts by 1
            
            if (shipType == ShipType.player)
                game.popupListHandler.ShowPopup(game.popupListHandler.warning_ImpactDamage, true, 0, 2);
        }
        else if (collisionMagnitude >= mediumHitCollisionMagnitudeThreshold)
        {
            float volumeMult = (collisionMagnitude / mediumHitCollisionMagnitudeThreshold) * 0.3f;
            ObjectPoolManager.SpawnObject(game.prefabs.mediumHitSFX, cp.point, volumeMult);
        }
        else if (collisionMagnitude >= lightHitCollisionMagnitudeThreshold)
        {
            float volumeMult = (collisionMagnitude / lightHitCollisionMagnitudeThreshold) * 0.3f;
            ObjectPoolManager.SpawnObject(game.prefabs.lightHitSFX, cp.point, volumeMult);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Sun"))
        {
            KillShip();
        }
    }
    
#endregion

#region RepairStation

    public SpaceshipPart[] GetKilledParts() => allParts.Where(part => part.IsKilled).ToArray();

    public void ClearAllModifiers()
    {
        spaceshipController.ClearModifiers();
    }

    public void Heal()
    {
        shipHealth = maxShipHealth;
        
        if (shipType == ShipType.player)
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
    
#endregion
}
