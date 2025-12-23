using System;
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

    GameManager game;
    
    private void Awake()
    {
        game = GameManager.I;
        
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
        sensitivitySlider.value = SaveManager.I.saveData.sensitivity;
        masterSlider.value = SaveManager.I.saveData.masterVolume;
        SFXSlider.value = SaveManager.I.saveData.SFXVolume;
        ambienceSlider.value = SaveManager.I.saveData.ambienceVolume;
        musicSlider.value = SaveManager.I.saveData.musicVolume;
    }

    private void Save()
    {
        SaveManager.I.saveData.sensitivity = sensitivitySlider.value;
        SaveManager.I.saveData.masterVolume = masterSlider.value;
        SaveManager.I.saveData.SFXVolume = SFXSlider.value;
        SaveManager.I.saveData.ambienceVolume = ambienceSlider.value;
        SaveManager.I.saveData.musicVolume = musicSlider.value;
    }
}
