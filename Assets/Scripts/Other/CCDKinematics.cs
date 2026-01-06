using System;
using System.Collections;
using UnityEngine;

public class CCDKinematics : MonoBehaviour
{
    [SerializeField] private Joint[] jointsTipToBase;
    [SerializeField, Tooltip("last joint")] private Transform effector;
    [SerializeField] private Transform goal;

    public Vector3 originalGoalLocalPosition {get; private set;}
    public Vector3 originalGoalPosition {get; private set;}
    public bool isBusy {get; private set;}
    public Vector3 goalPosition => goal.position;

    GameManager game;

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
        game = GameManager.I;
        
        originalGoalLocalPosition = goal.localPosition;
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

    public void MoveGoalPosition(Vector3 target, float speed)
    {
        goal.position = Vector3.MoveTowards(goal.position, target, speed * Time.deltaTime);
    }
    
    public float SetGoalPositionSmooth(Vector3 position, float moveSpeed, Action<Vector3, float> action = null)
    {
        float time = Vector3.Distance(goal.position, position) / moveSpeed;
        StopAllCoroutines();
        StartCoroutine(SmoothMove(position, time, action));
        return time;
    }
    
    /*public void SetGoalPositionSmooth(Transform _transform, float time, Func<Vector3, float, int> action = null)
    {
        StopAllCoroutines();
        StartCoroutine(SmoothMove(_transform, time, action));
    }*/

    private IEnumerator SmoothMove(Vector3 targetPosition, float time, Action<Vector3, float> action)
    {
        isBusy = true;
        
        float timer = 0;
        Vector3 start = goal.position;
        Vector3 lastPosition = start;

        while (timer < time)
        {
            timer += Time.deltaTime;
            
            goal.position = Vector3.Lerp(start, targetPosition, timer / time);
            action?.Invoke(goal.position - lastPosition, timer / time);
            lastPosition = goal.position;
            
            yield return null;
        }
        
        isBusy = false;
    }
    
    /*private IEnumerator SmoothMove(Transform targetTransform, float time, Func<Vector3, float, int> action)
    {
        isBusy = true;
        
        float timer = 0;
        Vector3 start = goal.position;
        Vector3 lastPosition = start;

        while (timer < time)
        {
            timer += Time.deltaTime;
            
            goal.position = Vector3.Lerp(start, targetTransform.position, timer / time);
            action?.Invoke(goal.position - lastPosition, timer / time);
            lastPosition = goal.position;
            
            yield return null;
        }
        
        isBusy = false;
    }*/
}
