using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance =null;
    public Page gamePages;
    public Sprite[] levels;
    public Image[] mission;
    public ControlAVMedia[] controlVideo;
    public ControlImageLoop imageLoop;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private void Init()
    {
        this.nextMission();

        foreach (var video in controlVideo)
        {
            if (video != null)
            {
                video.rewindVideo();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.changePage(0);
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            this.changePage(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            this.changePage(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            this.changePage(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            this.changePage(3);
        }


        if(this.gamePages.currentPageId == 0)
        {
            if (Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.reloadScene();
            }
        }
    }

    public void nextMission()
    {
        if(ConfigPage.Instance != null)
        {
            if (ConfigPage.Instance.gameLevelId < this.levels.Length-1)
            {
                ConfigPage.Instance.gameLevelId += 1;
            }
            else
            {
                ConfigPage.Instance.gameLevelId = 0;
            }
            for (int i = 0; i < mission.Length; i++)
            {
                if (mission[i] != null) 
                    mission[i].sprite = this.levels[ConfigPage.Instance.gameLevelId];
            }
        }
    }

    void randomMission()
    {
        var randomId = Random.Range(0, this.levels.Length);

        for(int i=0; i< mission.Length; i++)
        {
            if(mission[i] != null) mission[i].sprite = this.levels[randomId];
        }
    }

    public int CurrentPageId
    {
        get
        {
            return this.gamePages.currentPageId;
        }
    }

    public bool IsDetectionPages()
    {
        bool isDetected = false;
         if (this.gamePages.currentPageId == 0 || 
             this.gamePages.currentPageId == 2)
         {
            isDetected = true;
         }
         return isDetected;
    }

    public bool IsAnyVideoPlaying()
    {
        bool isPlaying = false;

        foreach(var video in controlVideo)
        {
            if(video.IsPlaying)
                isPlaying = true;
        }
        return isPlaying;

    }

    public void checkDetection(bool hasRed)
    {
        if (hasRed)
        {
            this.changePage(1);
        }
        else
        {
            this.changePage(this.gamePages.pages.Length - 1);
        }
    }

    public void reloadScene()
    {
        Debug.Log("Reload Scene");
        SceneManager.LoadScene(1);
    }

    public void changePage(int toPageId)
    {
        if(this.gamePages.currentPageId != toPageId) {
            this.gamePages.setPage(toPageId);
            switch (toPageId)
            {
                case 0:
                    Debug.Log("Idling Page");
                    this.Init();
                    break;
                case 1:
                    Debug.Log("Instruction Page");
                    if (controlVideo[0] != null) controlVideo[0].playVideo();
                    break;
                case 2:
                    Debug.Log("Game Image Loop Page");
                    if(imageLoop != null) imageLoop.enableLooping();
                    break;
                case 3:
                    Debug.Log("Ending Page");
                    if (imageLoop != null) imageLoop.stopLooping();
                    if (controlVideo[1] != null) controlVideo[1].playVideo();
                    break;
                case -1:
                    this.reloadScene();
                    break;
            }
        }
    }
}
