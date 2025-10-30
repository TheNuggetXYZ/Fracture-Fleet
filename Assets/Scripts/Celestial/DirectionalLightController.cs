using UnityEngine;

public class DirectionalLightController : MonoBehaviour
{
    private void Update()
    {
        if (!Camera.main) return;
        
        Vector3 camPos = Camera.main.transform.position;
        
        transform.LookAt(camPos);
    }
}
