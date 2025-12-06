using UnityEngine;

[CreateAssetMenu(fileName = "PrefabAtlas", menuName = "Scriptable Objects/PrefabAtlas")]
public class PrefabAtlas : ScriptableObject
{
    [Header("VFX")]
    public Transform metalSparkVFX;
    public Transform bulletHitVFX;
    public Transform shipDeathExplosionVFX;
    
    [Header("SFX")]
    public AudioSource shipShootSFX;
}
