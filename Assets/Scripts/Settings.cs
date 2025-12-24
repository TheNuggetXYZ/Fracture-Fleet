using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private WorldMenu worldMenu;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider ambienceSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle VSyncToggle;
    [SerializeField] private Toggle limitFPSToggle;
    [SerializeField] private TMP_InputField maxFPSInputField;

    GameManager game;
    SaveManager save;
    
    private void Awake()
    {
        game = GameManager.I;
        save = SaveManager.I;
        
        Load();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnEnable()
    {
        worldMenu.OnMenuOpen += Load;
        worldMenu.OnMenuClose += Save;
    }
    
    private void OnDisable()
    {
        worldMenu.OnMenuOpen -= Load;
        worldMenu.OnMenuClose -= Save;
    }

    private void Update()
    {
        game.audioMixer.SetFloat("MasterVolume", SliderToDecibel(masterSlider));
        game.audioMixer.SetFloat("SFXVolume", SliderToDecibel(SFXSlider));
        game.audioMixer.SetFloat("AmbienceVolume", SliderToDecibel(ambienceSlider));
        game.audioMixer.SetFloat("MusicVolume", SliderToDecibel(musicSlider));

        if (VSyncToggle.isOn)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;

        if (!limitFPSToggle.isOn)
            Application.targetFrameRate = -1;
        else if (int.TryParse(maxFPSInputField.text, out int maxFPS))
            Application.targetFrameRate = maxFPS;
    }

    private float SliderToDecibel(Slider slider)
    {
        return Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value) * 100 - 80;
    }

    private float SliderToSensitivity(Slider slider)
    {
        return Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value) * 300;
    }
    
    private void Load()
    {
        sensitivitySlider.value = save.saveData.sensitivity;
        masterSlider.value = save.saveData.masterVolume;
        SFXSlider.value = save.saveData.SFXVolume;
        ambienceSlider.value = save.saveData.ambienceVolume;
        musicSlider.value = save.saveData.musicVolume;
        VSyncToggle.isOn = save.saveData.VSync;
        limitFPSToggle.isOn = save.saveData.limitFPS;
        maxFPSInputField.text = save.saveData.maxFPS.ToString();
    }

    private void Save()
    {
        save.saveData.sensitivity = sensitivitySlider.value;
        save.saveData.masterVolume = masterSlider.value;
        save.saveData.SFXVolume = SFXSlider.value;
        save.saveData.ambienceVolume = ambienceSlider.value;
        save.saveData.musicVolume = musicSlider.value;
        save.saveData.VSync = VSyncToggle.isOn;
        save.saveData.limitFPS = limitFPSToggle.isOn;
        if (int.TryParse(maxFPSInputField.text, out int maxFPS))
            save.saveData.maxFPS = maxFPS;
    }
}
