using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class ObjectPoolManager : MonoBehaviour
{
    private static readonly Dictionary<string, PooledObjectInfo<Component>> objectPools = new();

    private static T FindObject<T>(T spawnObject, Vector3 spawnPosition, Quaternion spawnRotation = default, Transform parent = null, bool dontDestroyOnLoad = true) where T : Component
    {
        string key = spawnObject.gameObject.name;

        if (!objectPools.TryGetValue(key, out PooledObjectInfo<Component> pool)) // find or create pool for this object
        {
            pool = new PooledObjectInfo<Component> { component = typeof(T), poolParent = new GameObject(key.Replace("(Clone)", string.Empty)).transform };
            objectPools.Add(key, pool);
        }

        T spawnableObject = pool.GetInactiveObject(spawnPosition, spawnRotation) as T;
        if (!spawnableObject)
        {
            spawnableObject = Instantiate(spawnObject, spawnPosition, spawnRotation, !parent ? pool.poolParent : parent);

            if (!parent && dontDestroyOnLoad)
                DontDestroyOnLoad(spawnableObject.transform.parent);
        }

        return spawnableObject;
    }

    public static GameObject SpawnObject(GameObject spawnObject, Vector3 spawnPosition = default, Quaternion spawnRotation = default, Transform parent = null, bool dontDestroyOnload = true)
    {
        return FindObject(spawnObject.transform, spawnPosition, spawnRotation, parent, dontDestroyOnload).gameObject;
    }
    
    public static CannonBullet SpawnObject(CannonBullet spawnObject, Transform sender, float initialVelocity, Vector3 spawnPosition, Quaternion spawnRotation, Transform parent = null, bool dontDestroyOnload = false)
    {
        CannonBullet cb = FindObject(spawnObject, spawnPosition, spawnRotation, parent, dontDestroyOnload);
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

    public T GetInactiveObject(Vector3 position, Quaternion rotation)
    {
        if (inactiveObjects.Count > 0 && !inactiveObjects[0].IsDestroyed())
        {
            T obj = inactiveObjects[0];
            inactiveObjects.RemoveAt(0);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);
            return obj;
        }
        return null;
    }
}