using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Class and collaborated interfaces created by following tutorial found at: https://www.youtube.com/watch?v=aUi9aijvpgs
/// </summary>

public class DataPersistanceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] string fileName;
    [SerializeField] bool usingEncryption;

    GameData gameData;
    List<IDataPersistance> dataPersistanceObjects;
    FileDataHandler dataHandler;
    public static DataPersistanceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one Data Persistance Manager in the scene");
        }
        Instance = this;
    }
    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, usingEncryption); // Application.persistentDataPath gives the OS's standard directory for persisting data in a Unity Game
        this.dataPersistanceObjects = FindAllDataPersistanceObjects();
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }
    public void LoadGame()
    {
        // Load any saved data from file using data handler
        this.gameData = dataHandler.Load();

        // If no data found, init NewGame()
        if(this.gameData == null)
        {
            Debug.LogError("No data found. Iniialising to a new game");
            NewGame();
        }

        foreach(IDataPersistance obj in dataPersistanceObjects)
        {
            obj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        foreach (IDataPersistance obj in dataPersistanceObjects)
        {
            obj.SaveData(ref gameData);
        }

        // Save data to file using data handler
        dataHandler.Save(gameData);
    }

    List<IDataPersistance> FindAllDataPersistanceObjects()
    {
        IEnumerable<IDataPersistance> dataPersistanceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance>();
        return new List<IDataPersistance>(dataPersistanceObjects);
    }
}
