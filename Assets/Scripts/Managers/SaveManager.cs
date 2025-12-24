using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager I;
    
    public SaveData saveData {get; private set;}
    private string saveFilePath;

    private void Awake()
    {
        if (I == null)
            I = this;
        else
        {
            Debug.LogError("More than one instance!");
            Destroy(this);
        }
        
        saveFilePath = Application.persistentDataPath + "/FF.save";
        saveData = new SaveData();
        Load();
    }
    
    private void OnDestroy()
    {
        Save();
        I = null;
    }

    private void Save()
    {
        File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData, true));
    }

    private void Load()
    {
        if (File.Exists(saveFilePath))
            saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFilePath));
        else
            CreateSaveFile();
        
        if (saveData == null) // if the file is empty
            CreateSaveFile();
    }

    private void CreateSaveFile()
    {
        // set defaults and create the save
        saveData = new SaveData();
            
        Save();
    }
    
    public class SaveData
    {
        public float sensitivity = 100;
        public float masterVolume = 80;
        public float SFXVolume = 80;
        public float ambienceVolume = 80;
        public float musicVolume = 80;
        public bool VSync = true;
        public bool limitFPS = false;
        public int maxFPS = 60;
    }
}
