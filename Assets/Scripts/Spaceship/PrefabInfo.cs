using UnityEditor;
using UnityEngine;

public class PrefabInfo : MonoBehaviour
{
    public string id;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = GUID.Generate().ToString();
    }
}
