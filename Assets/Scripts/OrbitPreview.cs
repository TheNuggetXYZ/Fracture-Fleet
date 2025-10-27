#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class OrbitPreview : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private SpaceGravitySimulator spaceGravitySimulator;
    
    [Tooltip("Number of steps to simulate forward.")]
    [SerializeField] private int steps = 500;

    [Tooltip("Time (in seconds) for each step. Should roughly match Time.fixedDeltaTime.")]
    [SerializeField] private float stepTime = 0.02f;

    [Tooltip("Color of the orbit preview lines.")]
    [SerializeField] private Color orbitColor = Color.cyan;

    [Tooltip("Draw Gizmos even when not selected.")]
    [SerializeField] private bool alwaysVisible = true;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
        
        if (!alwaysVisible && !UnityEditor.Selection.Contains(gameObject))
            return;

        var spaceObjects = FindObjectsByType<SpaceObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (spaceObjects.Length == 0)
            return;

        // Copy simulation data from scene objects
        int count = spaceObjects.Length;
        Vector3[] positions = new Vector3[count];
        Vector3[] velocities = new Vector3[count];
        float[] masses = new float[count];

        for (int i = 0; i < count; i++)
        {
            positions[i] = spaceObjects[i].transform.position;
            velocities[i] = spaceObjects[i].initialVelocity;
            masses[i] = spaceObjects[i].mass;
        }

        // Prepare trails
        List<Vector3>[] trails = new List<Vector3>[count];
        for (int i = 0; i < count; i++)
            trails[i] = new List<Vector3> { positions[i] };

        float G = spaceGravitySimulator.gravitationalConstant;

        // Run simplified simulation loop (matching your actual physics)
        for (int step = 0; step < steps; step++)
        {
            // Apply velocity updates (same as your UpdateVelocity)
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i == j || spaceObjects[i].lockedPosition) continue;

                    Vector3 direction = (positions[j] - positions[i]).normalized;
                    float distanceSqr = (positions[j] - positions[i]).sqrMagnitude;
                    Vector3 force = direction * (G * (masses[i] * masses[j] / distanceSqr));
                    Vector3 acceleration = force / masses[i];

                    velocities[i] += acceleration * stepTime;
                }
            }

            // Update positions
            for (int i = 0; i < count; i++)
            {
                positions[i] += velocities[i] * stepTime;
                trails[i].Add(positions[i]);
            }
        }

        // Draw orbit lines
        Gizmos.color = orbitColor;
        for (int i = 0; i < count; i++)
        {
            var trail = trails[i];
            for (int j = 1; j < trail.Count; j++)
                Gizmos.DrawLine(trail[j - 1], trail[j]);
        }
    }
}
#endif