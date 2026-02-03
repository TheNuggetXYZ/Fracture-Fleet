using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    [SerializeField] private Image image;
    [field: SerializeField] public IndicatorData enemyIndicator {get; private set;}
    [field: SerializeField] public IndicatorData comradeIndicator {get; private set;}

    private IndicatorData indicatorData;
    
    [System.Serializable]
    public class IndicatorData
    {
        public Sprite sprite;
        public Color color;
        public Vector2 transparentToOpaqueDistanceRange;
    }

    public void SetData(IndicatorData data)
    {
        indicatorData = data;
        
        image.sprite = data.sprite;
        image.color = new Color(data.color.r, data.color.g, data.color.b, image.color.a);
    }

    public void SetDistanceFromTarget(float distance)
    {
        float alpha = Mathf.InverseLerp(indicatorData.transparentToOpaqueDistanceRange.x, indicatorData.transparentToOpaqueDistanceRange.y, distance);
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
    }
}
