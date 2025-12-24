using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.Audio;

public class ObjectPoolManager : MonoBehaviour
{
    private static readonly Dictionary<string, PooledObjectInfo<Component>> objectPools = new();

    private static T FindObject<T>(T spawnObject, Vector3 spawnPosition, Quaternion spawnRotation, Vector3 spawnSize, Transform parent, bool dontDestroyOnLoad) where T : Component
    {
        string key = spawnObject.gameObject.name;

        if (!objectPools.TryGetValue(key, out PooledObjectInfo<Component> pool)) // find or create pool for this object
        {
            pool = new PooledObjectInfo<Component> { component = typeof(T), poolParent = new GameObject(key.Replace("(Clone)", string.Empty)).transform };
            objectPools.Add(key, pool);
        }

        T spawnableObject = pool.GetInactiveObject(spawnPosition, spawnRotation, spawnSize) as T;
        if (!spawnableObject)
        {
            spawnableObject = Instantiate(spawnObject, spawnPosition, spawnRotation, !parent ? pool.poolParent : parent);
            spawnableObject.transform.localScale = spawnSize;

            if (!parent && dontDestroyOnLoad)
                DontDestroyOnLoad(spawnableObject.transform.parent);
        }
        else if (parent)
            spawnableObject.transform.parent = parent;

        return spawnableObject;
    }

    public static Transform SpawnObject(Transform spawnObject, Vector3 spawnPosition = default, Quaternion spawnRotation = default, Vector3? spawnSize = null, Transform parent = null, bool dontDestroyOnload = true)
    {
        Transform obj = FindObject(spawnObject, spawnPosition, spawnRotation, spawnSize ?? spawnObject.transform.localScale, parent, dontDestroyOnload);
        return obj;
    }
    
    public static AudioObject SpawnObject(AudioObject spawnObject, Vector3 spawnPosition = default, float volumeMultiplier = 1, float randomPitchAmount = 0.2f, bool distanceCheck = true, Quaternion spawnRotation = default, Vector3? spawnSize = null, Transform parent = null, bool dontDestroyOnload = true)
    {
        if (distanceCheck && spawnObject.AudioSource.spatialBlend > 0 && Camera.main && Vector3.Distance(spawnPosition, Camera.main.transform.position) > spawnObject.AudioSource.maxDistance)
            return null;
        
        AudioObject obj = FindObject(spawnObject, spawnPosition, spawnRotation, spawnSize ?? spawnObject.transform.localScale, parent, dontDestroyOnload);
        obj.SetVolumeMultiplier(volumeMultiplier);
        obj.AddPitch(Random.Range(-randomPitchAmount, randomPitchAmount));
        return obj;
    }
    
    public static CannonBullet SpawnObject(CannonBullet spawnObject, Transform sender, Vector3 initialVelocity, Vector3 spawnPosition, Quaternion spawnRotation, Vector3? spawnSize = null, Transform parent = null, bool dontDestroyOnload = false)
    {
        CannonBullet cb = FindObject(spawnObject, spawnPosition, spawnRotation, spawnSize ?? spawnObject.transform.localScale, parent, dontDestroyOnload);
        cb.Initialize(sender, initialVelocity);
        return cb;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string objNameModified = obj.name.Replace("(Clone)", string.Empty); // removes "(clone)" from object's name

        if (objectPools.TryGetValue(objNameModified, out PooledObjectInfo<Component> pool))
        {
            obj.SetActive(false);

            pool.inactiveObjects.Add(obj.GetComponent(pool.component));
        }
    }
}

public class PooledObjectInfo<T> where T : Component
{
    public Transform poolParent;
    public System.Type component;
    public readonly List<T> inactiveObjects = new();

    public T GetInactiveObject(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (inactiveObjects.Count > 0 && !inactiveObjects[0].IsDestroyed())
        {
            T obj = inactiveObjects[0];
            inactiveObjects.RemoveAt(0);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.transform.localScale = scale;
            obj.gameObject.SetActive(true);
            return obj;
        }
        return null;
    }
}