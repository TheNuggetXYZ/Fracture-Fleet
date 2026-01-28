using UnityEngine;

[CreateAssetMenu(fileName = "ShipModels", menuName = "Scriptable Objects/ShipModels")]
public class ShipModelsSO : ScriptableObject
{
    public ShipModel[] shipModels;
    
    [System.Serializable]
    public class ShipModel
    {
        [field: SerializeField] public SpaceshipPartManager shipPrefab { get; private set; }
        [SerializeField] private ShipPartModel[] partModels;

        public void Assign(ShipPartModel[] partModels)
        {
            this.partModels = partModels;
        }
        
        public ShipPartModel[] PartModels => partModels;
    }
    
    [System.Serializable]
    public class ShipPartModel
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 lossyScale;
        public GameObject prefab;
        public string prefabInfoID;
    }
}
