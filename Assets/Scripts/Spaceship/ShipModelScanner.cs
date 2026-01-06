using UnityEditor;
using UnityEngine;

public class ShipModelScanner : MonoBehaviour
{
    [SerializeField] private ShipModelsSO shipModels;
    [SerializeField] private Transform mainShipTransform;
    [SerializeField] private int modelNumber;
    [SerializeField] private bool scan;

    private void OnValidate()
    {
        if (scan)
        {
            scan = false;
            
            ScanModel();
        }
    }

    private void ScanModel()
    {
        mainShipTransform.position = Vector3.zero;
        
        int kidAmount = transform.childCount;
        ShipModelsSO.ShipPartModel[] partModels = new ShipModelsSO.ShipPartModel[kidAmount];

        int i = 0;
        foreach (var part in transform.GetComponentsInChildren<SpaceshipPart>())
        {
            Transform t = part.transform;
            partModels[i] = new ShipModelsSO.ShipPartModel();
            partModels[i].position  = t.position;
            partModels[i].rotation = t.rotation;
            partModels[i].lossyScale =  t.lossyScale;
            partModels[i].prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(part.gameObject);
            if (partModels[i].prefab == null)
                Debug.LogError("Prefab not found");
            
            i++;
        }
        
        shipModels.shipModels[modelNumber].Assign(partModels);
    }
}
