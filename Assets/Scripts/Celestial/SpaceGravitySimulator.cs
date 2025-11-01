using UnityEngine;

public class SpaceGravitySimulator : MonoBehaviour
{
    public static SpaceGravitySimulator Instance;
    
    [field: SerializeField] public float gravitationalConstant {get; private set;}
    [SerializeField] public const float densityMultiplier = 0.001f;
    
    private SpaceObject[] spaceObjects;
    private SpaceshipGravity[] spaceships;

    private void Awake()
    {
        Instance = this;
        
        spaceObjects = FindObjectsByType<SpaceObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        spaceships = FindObjectsByType<SpaceshipGravity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void FixedUpdate()
    {
        foreach (SpaceObject spaceObject in spaceObjects)
        {
            foreach (SpaceObject otherSpaceObject in spaceObjects)
            {
                if (spaceObject != otherSpaceObject)
                    spaceObject.UpdateVelocity(otherSpaceObject);
            }
            
            spaceObject.UpdatePosition();

            foreach (SpaceshipGravity spaceship in spaceships)
            {
                spaceship.ApplyGravity(spaceObject);
            }
        }
    }
}
