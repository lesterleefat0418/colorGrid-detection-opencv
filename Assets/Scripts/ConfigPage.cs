using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfigPage : MonoBehaviour
{
    public static ConfigPage Instance = null;
    public ConfigData configData;
    public int gameLevelId = 0;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;

        DontDestroyOnLoad(this);
        this.LoadRecords();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("Switch Scene");

        }*/
    }

    public void LoadRecords()
    {
        if (DataManager.Load() != null)
        {
            this.configData = DataManager.Load();
            //Debug.Log("Load config file: " + configData);
            

            this.changeScene(1);
        }
        else
        {
            Debug.Log("config file is empty and get data from inspector!");
            this.configData.blackThreshold = 0.35f;
            this.configData.redRange_head.x = 0;
            this.configData.redRange_head.y = 11;
            this.configData.redRange_end.x = 120;
            this.configData.redRange_end.y = 180;
            this.configData.blueRange.x = 60;
            this.configData.blueRange.y = 120;
            this.configData.yellowRange.x = 11;
            this.configData.yellowRange.y = 60;
            this.configData.maxRed = 3;
            this.configData.enterRedCheckDelays = 2f;
            this.configData.leaveRedCheckDelays = 3f;
            this.configData.fullscreen = false;
            this.configData.markersPosition[0].position = new Vector3(-443f, 0f, 0f);
            this.configData.markersPosition[1].position = new Vector3(108f, 0f, 0f);
            this.configData.markersPosition[2].position = new Vector3(-443f, -282f, 0f);
            this.configData.markersPosition[3].position = new Vector3(108f, -282f, 0f);
            this.SaveRecords();
        }
    }

    public void Save()
    {
        DataManager.Save(this.configData);
    }

    public void SaveRecords()
    {
        DataManager.Save(this.configData);
        this.LoadRecords();
    }

    public void changeScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }


    private void OnDisable()
    {
        this.SaveRecords();
    }


    private void OnApplicationQuit()
    {
        this.SaveRecords();
    }
}


[System.Serializable]
public class ConfigData
{
    public float blackThreshold;
    public Vector2Int redRange_head;
    public Vector2Int redRange_end;
    public Vector2Int blueRange;
    public Vector2Int yellowRange;
    public int maxRed;
    public float enterRedCheckDelays;
    public float leaveRedCheckDelays;
    public bool fullscreen;
    public Marker[] markersPosition;
}

public static class DataManager
{
    public static string directory = Directory.GetCurrentDirectory();
    public static string fileName = "/config.txt";
    public static void Save(ConfigData sData, bool dataMultipleLines = true)
    {
        string json = JsonUtility.ToJson(sData, dataMultipleLines);
        File.WriteAllText(directory + fileName, json);

        Debug.Log("Saved config file");
    }

    public static ConfigData Load()
    {
        string fullPath = directory + fileName;
        ConfigData loadData = new ConfigData();

        if (File.Exists(fullPath))
        {
            if (new FileInfo(fileName.Replace("/", "")).Length != 0)
            {
                string json = File.ReadAllText(fullPath);
                loadData = JsonUtility.FromJson<ConfigData>(json);
                return loadData;
            }
            else
            {
                Debug.Log("Empty File");
                return null;
            }
        }
        else
        {
            Debug.Log("Save File does not exist & create new One");
            var newFile = File.Create(fullPath);
            newFile.Close();
            return null;
        }
    }

}

[System.Serializable]
public class Marker
{
    public string name;
    public Vector3 position;

}
