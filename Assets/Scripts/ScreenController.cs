using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using System;

[RequireComponent(typeof(FocusWindow))]
public class ScreenController : Singleton<ScreenController>
{
    public Vector2Int[] resolution;
    public bool mouseStatus = true;
    public bool enableFocusWindow = true;
    private FocusWindow focusWindow = null;
    //[SerializeField] private float _hudRefreshRate = 0.1f;
    // Start is called before the first frame update

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }

    private void Start()
    {

        for (int i = 0; i < Display.displays.Length; i++)
        {
            if(Display.displays[i] != null) { 
                Display.displays[i].Activate();
                Display.displays[i].SetRenderingResolution(resolution[i].x, resolution[i].y);
            }
        }
        Cursor.visible = this.mouseStatus;
        focusWindow = GetComponent<FocusWindow>();
        focusWindow.isOn = this.enableFocusWindow;

        //StartCoroutine(countFPS());
      
    }

    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.F1))
        {
            mouseStatus = !this.mouseStatus;
            Cursor.visible = mouseStatus;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            this.enableFocusWindow = !this.enableFocusWindow;
            focusWindow.isOn = this.enableFocusWindow;
        }
        else if(Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reset");
            SceneManager.LoadScene(1);
        }       
    }


    /*private IEnumerator countFPS()
    {
        while (true)
        {
            this.FPS = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(_hudRefreshRate);
        }
    }

    private float _fps;
    public float FPS
    {
        get
        {
            return _fps;
        }
        set
        {
            this._fps = value;
        }
    }*/

}


[Serializable]
public class Page
{
    public CanvasGroup[] pages;
    public int currentPageId;
    public bool isAnimated = false;
    public float duration = 0.5f;

    public void Init()
    {
        this.currentPageId = 0;
        this.setPage(this.currentPageId);
        this.isAnimated = false;
    }

    public void ControlPage(int id, bool status)
    {
        if (pages[id] != null)
        {
            if (status == true)
            {
                pages[id].DOFade(1f, 1f).OnComplete(() => this.isAnimated = false);
                pages[id].interactable = true;
                pages[id].blocksRaycasts = true;
            }
            else
            {
                pages[id].DOFade(0f, 0.5f).OnComplete(() => this.isAnimated = false);
                pages[id].interactable = false;
                pages[id].blocksRaycasts = false;
            }
        }
    }

    public void setPage(int id)
    {
        if (!this.isAnimated)
        {
            this.isAnimated = true;
            this.currentPageId = id;
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    if (i == id)
                    {
                        pages[id].DOFade(1f, this.duration).OnComplete(() => this.isAnimated = false);
                        pages[id].interactable = true;
                        pages[id].blocksRaycasts = true;
                    }
                    else
                    {
                        pages[i].DOFade(0f, 0f);
                        pages[i].interactable = false;
                        pages[i].blocksRaycasts = false;
                    }
                }
            }
        }
    }

}
