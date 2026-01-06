using System.Collections.Generic;
using UnityEngine;

public class SpaceshipPart : MonoBehaviour
{
    [field: SerializeField] public int ID { get; private set; }
    [SerializeField] private bool unkillable;
    
    [SerializeField] private Transform _mainPart;
    [SerializeField] private Collider partCollider;
    
    [SerializeField] private int partHealth = 1;
    [SerializeField] private float partMass = 1;
    
    [SerializeField] private OnKillModifierType onKillModifier;
    [SerializeField] private float onKillModifierValue;
    
    [SerializeField] private SpaceshipPart partToAlsoKill;

    private const int SCRAP_LAYER = 8;
    
    private Transform mainPart => !_mainPart.Equals(null) || _mainPart != null ? _mainPart : transform;
    public Collider PartCollider => partCollider;
    public int PartHealth => partHealth;
    public bool IsUnkillable => unkillable;
    public bool IsKilled => isKilled;
    public Vector3 OriginalPositionLocalToParent => localOriginalPosition;
    public Transform OriginalParent => isKilled ? originalParent : transform.parent;
    
    public enum OnKillModifierType
    {
        none = 0,
        rotation = 1,
        unstableRotation = 3,
        speed = 2,
    }

    private List<SpaceshipPart> killedParts = new();
    private bool isKilled;
    private Rigidbody addedRb;

    private bool originalIsTrigger;
    private Vector3 localOriginalPosition;
    private Quaternion originalLocalRotation;
    private Transform originalParent;
    private int originalLayer;
    
    GameManager game;

    protected void Awake()
    {
        game = GameManager.I;
    }

    public void TakeDamage(int damage)
    {
        partHealth -= damage;
    }

    public void Kill(Vector3 velocity, bool forceKill, out bool successfullyKilled)
    {
        successfullyKilled = false;
        
        if (isKilled || (unkillable && !forceKill)) return;
        
        isKilled = true;

        originalParent = mainPart.parent;
        localOriginalPosition = mainPart.localPosition;
        originalLocalRotation = mainPart.localRotation;
        originalLayer = mainPart.gameObject.layer;

        mainPart.gameObject.layer = SCRAP_LAYER;

        if (!addedRb)
        {
            addedRb = mainPart.gameObject.AddComponent<Rigidbody>();
            addedRb.linearVelocity = velocity;
            addedRb.useGravity = false;
            addedRb.mass = partMass;
            addedRb.transform.parent = game.hierarchyManager.folder_scrap;
        }
        
        if (partCollider)
        {
            originalIsTrigger = partCollider.isTrigger;
            
            partCollider.isTrigger = false; // make sure it now has collisions if it didn't previously
            partCollider.gameObject.layer = SCRAP_LAYER;
        }
        
        ObjectPoolManager.SpawnObject(game.prefabs.partLostSFX, mainPart.position);
        
        killedParts.Add(this);
        successfullyKilled = true;
        
        partToAlsoKill?.Kill(velocity, false, out bool _);
    }

    public void GetOnKillModifiers(out OnKillModifierType[] _onKillModifiers, out float[] _onKillModifierValues)
    {
        int length = killedParts.Count;
        _onKillModifiers = new OnKillModifierType[length];
        _onKillModifierValues =  new float[length];

        for (int i = 0; i < length; i++)
        {
            _onKillModifiers[i] = killedParts[i].onKillModifier;
            _onKillModifierValues[i] = killedParts[i].onKillModifierValue;
        }
    }

    public void Repair(bool setPosition)
    {
        isKilled = false;

        mainPart.parent = originalParent;
        mainPart.gameObject.layer = originalLayer;
        
        if (setPosition)
            mainPart.localPosition = localOriginalPosition;
        
        mainPart.localRotation = originalLocalRotation;
        

        RemoveRigidbody();
        
        if (partCollider)
        {
            partCollider.isTrigger = originalIsTrigger;
            partCollider.gameObject.layer = originalLayer;
        }
        
        killedParts.Clear();
    }

    public void RemoveRigidbody()
    {
        if (addedRb)
        {
            Destroy(addedRb);
        }
    }

    public void TurnOffCollisions()
    {
        if (partCollider)
            partCollider.isTrigger = true;
    }
    
    public void SetPosition(Vector3 position) => mainPart.position = position;
}
