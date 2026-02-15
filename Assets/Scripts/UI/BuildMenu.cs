using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scrapReportText;
    [SerializeField] private Transform scrapReportGameObject;
    [SerializeField] private Button buildButton0;
    [SerializeField] private Button buildButton1;
    [SerializeField] private Button buildButton2;

    public Action<int> onBuildShip;

    private void OnEnable()
    {
        buildButton0.onClick.AddListener(BuildShip0);
        buildButton1.onClick.AddListener(BuildShip1);
        buildButton2.onClick.AddListener(BuildShip2);
    }

    private void OnDisable()
    {
        buildButton0.onClick.RemoveListener(BuildShip0);
        buildButton1.onClick.RemoveListener(BuildShip1);
        buildButton2.onClick.RemoveListener(BuildShip2);
    }
    
    private void BuildShip0() => BuildShip(0);
    private void BuildShip1() => BuildShip(1);
    private void BuildShip2() => BuildShip(2);

    private void BuildShip(int modelNumber)
    {
        onBuildShip?.Invoke(modelNumber);
    }

    public void OnFailBuildShip(string report)
    {
        scrapReportGameObject.gameObject.SetActive(true);
        scrapReportText.text = report;
    }
}
