using UnityEngine;

public class SpaceshipHealth : MonoBehaviour, ITakeDamage
{
    [SerializeField] private Rigidbody spaceshipRigidbody;
    [SerializeField] private SpaceshipPart[] killableParts;
    
    [System.Serializable]
    private class SpaceshipPart
    {
        public Transform mainPart;
        public Collider partCollider;
        public int partHealth = 1;
        
        [HideInInspector] public bool isKilled;
        
        public Transform partToAlsoKill;
        public Collider partToAlsoKillCollider;
    }
    
    public void TakeDamage(int damage, Transform hitCollider)
    {
        foreach (SpaceshipPart part in killableParts)
        {
            if (part.partCollider.transform == hitCollider)
            {
                part.partHealth -= damage;
                
                if (part.partHealth <= 0 && !part.isKilled)
                {
                    if (part.partToAlsoKill && part.partToAlsoKillCollider && part.partToAlsoKill.parent) // if it has a parent it hasn't been destroyed yet
                    {
                        SpaceshipPart partToKill = new SpaceshipPart();
                        partToKill.mainPart = part.partToAlsoKill;
                        partToKill.partCollider = part.partToAlsoKillCollider;
                        DestroySpaceshipPart(partToKill);
                    }
                    
                    DestroySpaceshipPart(part);
                }
            }
        }
    }

    private void DestroySpaceshipPart(SpaceshipPart part)
    {
        part.isKilled = true;
        Rigidbody partRb = part.mainPart.gameObject.AddComponent<Rigidbody>();
        
        if (!partRb)
            return;

        partRb.linearVelocity = spaceshipRigidbody.linearVelocity;
        partRb.transform.parent = null; // set parent to scene
        partRb.useGravity = false;
        part.partCollider.isTrigger = false; // make sure it now has collisions if it didn't previously
    }
}
