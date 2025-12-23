using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager I;
    
    public SaveData saveData {get; private set;}

    private void Awake()
    {
        if (I == null)
            I = this;
        else
        {
            Debug.LogError("More than one instance!");
            Destroy(this);
        }
        
        Load();
    }
    
    private void OnDestroy()
    {
        Save();
        I = null;
    }

    private void Load()
    {
        saveData = new SaveData();
    }

    private void Save()
    {
        
    }
    
    public class SaveData
    {
        public float sensitivity = 100;
        public float masterVolume = 80;
        public float SFXVolume = 80;
        public float ambienceVolume = 80;
        public float musicVolume = 80;
    }
}
