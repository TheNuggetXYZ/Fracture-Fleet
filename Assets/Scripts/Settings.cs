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
    }

    private void Save()
    {
        save.saveData.sensitivity = sensitivitySlider.value;
        save.saveData.masterVolume = masterSlider.value;
        save.saveData.SFXVolume = SFXSlider.value;
        save.saveData.ambienceVolume = ambienceSlider.value;
        save.saveData.musicVolume = musicSlider.value;
    }
}
