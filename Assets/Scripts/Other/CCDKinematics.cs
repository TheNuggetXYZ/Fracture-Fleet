using System;
using System.Collections;
using UnityEngine;

public class CCDKinematics : MonoBehaviour
{
    [SerializeField] private Joint[] jointsTipToBase;
    [SerializeField, Tooltip("last joint")] private Transform effector;
    [SerializeField] private Transform goal;

    public Vector3 originalGoalPosition {get; private set;}

    [System.Serializable]
    public struct Joint
    {
        public Transform transform;
        public Vector3 axis;
        
        public void RotateFromTo(Vector3 from, Vector3 to)
        {
            Quaternion delta = Quaternion.FromToRotation(from, to);
            transform.rotation = delta * transform.rotation;
        }
    }

    private void Awake()
    {
        originalGoalPosition = goal.position;
    }

    private void Update()
    {
        foreach (Joint joint in jointsTipToBase) 
        {
            // Point the effector towards the goal
            Vector3 directionToEffector = effector.position - joint.transform.position;
            Vector3 directionToGoal = goal.position - joint.transform.position;
            joint.RotateFromTo(directionToEffector, directionToGoal);
            
            // Constrain to rotate about the axis
            Vector3 curHingeAxis = joint.transform.rotation * joint.axis;
            Vector3 hingeAxis = joint.transform.parent.rotation * joint.axis;
            joint.RotateFromTo(curHingeAxis, hingeAxis);
            
            // Enforce Joint Limits
            //joint.localRotation.clampEuler(joint.minLimit, joint.maxLimit);
        }
    }

    public void SetGoalPosition(Vector3 position)
    {
        goal.position = position;
    }
    
    public void SetGoalPositionSmooth(Vector3 position, float time, Func<Vector3, float, int> action = null)
    {
        StopAllCoroutines();
        StartCoroutine(SmoothMove(position, time, action));
    }

    private IEnumerator SmoothMove(Vector3 position, float time, Func<Vector3, float, int> action)
    {
        float timer = 0;
        Vector3 start = goal.position;

        while (timer < time)
        {
            timer += Time.deltaTime;
            
            goal.position = Vector3.Lerp(start, position, timer / time);
            action?.Invoke(goal.position, timer / time);
            
            yield return null;
        }
    }
}
