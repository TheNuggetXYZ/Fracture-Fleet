using UnityEditor;
using UnityEngine;

public class PrefabInfo : MonoBehaviour
{
    public string id;

#if  UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = GUID.Generate().ToString();
    }
#endif
}
