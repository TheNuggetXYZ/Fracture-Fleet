using System.Collections.Generic;
using UnityEngine;

public class SpaceshipPart : MonoBehaviour
{
    [SerializeField] private bool unkillable;
    
    [SerializeField] private Transform mainPart;
    [SerializeField] private Collider partCollider;
    
    [SerializeField] private int partHealth = 1;
    [SerializeField] private float partMass = 1;
    
    [SerializeField] private OnKillModifierType onKillModifier;
    [SerializeField] private float onKillModifierValue;
    
    [SerializeField] private SpaceshipPart partToAlsoKill;
    
    private Transform MainPart => mainPart ? mainPart : transform;
    public Collider PartCollider => partCollider;
    public int PartHealth => partHealth;
    public bool IsUnkillable => unkillable;
    
    public enum OnKillModifierType
    {
        none = 0,
        rotation = 1,
        unstableRotation = 3,
        speed = 2,
    }

    private List<SpaceshipPart> killedParts = new();
    private bool isKilled;

    public void TakeDamage(int damage)
    {
        partHealth -= damage;
    }

    public void Kill(Vector3 velocity, bool forceKill, out bool successfullyKilled)
    {
        successfullyKilled = false;
        
        if (isKilled || (unkillable && !forceKill)) return;
        
        isKilled = true;
        
        Rigidbody partRb = MainPart.gameObject.AddComponent<Rigidbody>();
        partRb.linearVelocity = velocity;
        partRb.transform.parent = null; // set parent to scene
        partRb.useGravity = false;
        partRb.mass = partMass;
        if (partCollider)
            partCollider.isTrigger = false; // make sure it now has collisions if it didn't previously
        
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
}
