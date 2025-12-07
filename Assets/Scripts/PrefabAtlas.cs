using UnityEngine;

[CreateAssetMenu(fileName = "PrefabAtlas", menuName = "Scriptable Objects/PrefabAtlas")]
public class PrefabAtlas : ScriptableObject
{
    [Header("VFX")]
    public Transform metalSparkVFX;
    public Transform bulletHitVFX;
    public Transform shipDeathExplosionVFX;
    
    [Header("SFX")]
    public AudioObject shipShootSFX;
    public AudioObject lightHitSFX;
    public AudioObject mediumHitSFX;
    public AudioObject heavyHitSFXp1;
    public AudioObject heavyHitSFXp2;
    public AudioObject metalSparkSFX;
    
}
