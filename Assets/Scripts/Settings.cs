using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private Slider sensitivitySlider;

    public void OnEnable()
    {
        // LOAD
        sensitivitySlider.value = SaveManager.I.saveData.sensitivity;
    }

    public void Save()
    {
        // SAVE
        SaveManager.I.saveData.sensitivity = sensitivitySlider.value;
    }
}
