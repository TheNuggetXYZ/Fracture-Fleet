using System;
using UnityEngine;

public class SpaceshipPartManager : MonoBehaviour, ITakeDamage
{
    [SerializeField] private SpaceshipController spaceshipController;
    [SerializeField] private bool fetchChildParts;
    [SerializeField] private Transform childPartsParent;
    [SerializeField] private SpaceshipPart[] killableParts;
    [SerializeField] private float collisionMagnitudeThreshold;
    [SerializeField] private Transform metalSparkEffect;
    
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

                if (child && child.gameObject.activeInHierarchy )
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
        foreach (SpaceshipPart part in killableParts)
        {
            if (part && part.PartCollider.transform == hitCollider)
            {
                part.TakeDamage(damage);
                
                if (part.PartHealth <= 0)
                {
                    part.Kill(GetVelocity() + hitVelocity, out bool successfullyKilled);
                    
                    if (successfullyKilled)
                    {
                        part.GetOnKillModifiers(out SpaceshipPart.OnKillModifierType[] types, out float[] values);
                        ApplyOnKillModifiers(types, values);
                    }
                }
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
