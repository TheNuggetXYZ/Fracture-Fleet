using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceGravitySimulator : MonoBehaviour
{
    public static SpaceGravitySimulator I {get; private set;}
    
    [field: SerializeField] public float gravitationalConstant {get; private set;}
    [SerializeField] public const float densityMultiplier = 0.001f;
    [field: SerializeField] public SpaceObject mainBody {get; private set;}
    
    private SpaceObject[] celestials;
    private SpaceshipGravity[] spaceships;
    private List<Asteroid> asteroids = new();

    private void Awake()
    {
        if (I == null)
            I = this;
        else
        {
            Debug.LogError("More than one instance found!");
            Destroy(this);
        }
        
        celestials = FindCelestialBodies();
        spaceships = FindObjectsByType<SpaceshipGravity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void FixedUpdate()
    {
        foreach (SpaceshipGravity spaceship in spaceships)
        {
            spaceship.totalGravity = Vector3.zero;
        }
        
        foreach (SpaceObject spaceObject in celestials)
        {
            // CELESTIAL BODIES
            foreach (SpaceObject otherSpaceObject in celestials)
            {
                if (spaceObject != otherSpaceObject)
                    spaceObject.UpdateVelocity(otherSpaceObject);
            }

            // SPACESHIPS
            foreach (SpaceshipGravity spaceship in spaceships)
            {
                spaceship.ApplyGravity(spaceObject);
            }
        }

        // ASTEROIDS
        foreach (Asteroid asteroid in asteroids)
        {
            asteroid.UpdatePosition();
        }
    }

    public void AddAsteroid(Asteroid asteroid)
    {
        asteroids.Add(asteroid);
    }

    public static SpaceObject[] FindCelestialBodies()
    {
        return FindObjectsByType<SpaceObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(o => o.isCelestialBody).ToArray();
    }
}
