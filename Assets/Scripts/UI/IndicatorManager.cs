using System;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    [SerializeField] private Indicator indicatorPrefab;
    
    private List<Indicator> indicators = new();
    private List<SpaceshipPartManager> ships;

    GameManager game;

    private void Awake()
    {
        game = GameManager.I;
    }
    
    private void LateUpdate()
    {
        UpdateIndicators(game.spaceshipTracker.shipList);
    }

    private void UpdateIndicators(List<SpaceshipPartManager> ships)
    {
        if (indicators.Count < ships.Count)
        {
            for (int i = 0; i < ships.Count - indicators.Count; i++)
            {
                indicators.Add(Instantiate(indicatorPrefab, transform));
            }
        }

        if (indicators.Count > ships.Count)
        {
            for (int i = 0; i < indicators.Count - ships.Count; i++)
            {
                indicators[i + ships.Count].gameObject.SetActive(false);
            }
        }
        
        for (int i = 0; i < ships.Count; i++)
        {
            if (ships[i] == null)
            {
                indicators[i].gameObject.SetActive(false);
                continue;
            }
            
            Vector3 screenPos = Camera.main.WorldToScreenPoint(ships[i].transform.position);

            if (screenPos.z >= 0 && !ships[i].shipDead)
            {
                indicators[i].transform.position = screenPos;
                indicators[i].gameObject.SetActive(true);

                if (ships[i].shipType == SpaceshipPartManager.ShipType.enemy)
                    indicators[i].SetData(indicatorPrefab.enemyIndicator);
                else if (ships[i].shipType == SpaceshipPartManager.ShipType.comrade)
                    indicators[i].SetData(indicatorPrefab.comradeIndicator);
                
                indicators[i].SetDistanceFromTarget(Vector3.Distance(Camera.main.transform.position, ships[i].transform.position));
            }
            else
                indicators[i].gameObject.SetActive(false);
        }
    }
}
