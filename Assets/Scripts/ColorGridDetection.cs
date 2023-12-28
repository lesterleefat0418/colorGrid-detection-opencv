using UnityEngine;
using UnityEngine.SceneManagement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine.UI;
using OpenCVForUnity.ImgprocModule;
using System.IO;
using System;
using System.Collections.Generic;
using OpenCVForUnityExample;
using System.Text;

public class ColorGridDetection : MonoBehaviour
{
    public static ColorGridDetection Instance = null;
    public bool isFullscreen = false;
    public int width;
    public int height;
    public int cellWidth, cellHeight;
    public CanvasScaler canvasScaler;
    public FpsMonitor fpsMonitor;
    public RawImage webcamRawImage;
    public bool showMarker;
    public Camera targetCamera;
    public Corner[] markers;
    public RawImage croppedImage;
    public CanvasGroup[] canvasRemindPoints;
    private Mat croppedMat;

    public Vector3 topLeft = new Vector3(0, 0, 0);
    public Vector3 topRight = new Vector3(0, 0, 0);
    public Vector3 bottomLeft = new Vector3(0, 0, 0);
    public Vector3 bottomRight = new Vector3(0, 10, 0);
    public Vector2 margin = new Vector2(0f, 0f);
    public bool verticalFlip = false;
    public bool horizontalFlip = false;

    public SaveImage saveImage;
    private LineRenderer lineRenderer;
    private Texture2D croppedTexture = null;
    private Texture2D webcamTexture = null;
    private RenderTexture renderTexture = null;
    private MatOfPoint2f dstPoints = null, srcPoints=null;
    private UnityEngine.Rect readRect;
    public GameObject colorTextPrefab;
    public Transform parent;

    private RectTransform originalCroppedTexture;
    private GridLayoutGroup originalCroppedImageGridLayout;
    public Vector2 originalCroppedPosition;
    public Vector2 originalCroppedSizeDelta;
    private Vector3 halfScale = new Vector3(0.5f, 0.5f, 0.5f);
    public bool useGridDetect = false;
    //private ColorHSV customColor1HSV;
    //private ColorHSV customColor2HSV;
    //private ColorHSV customColor3HSV;
    //private ColorHSV blankHSV;
   // private ColorHSV pauseHSV;
    public int row = 4;
    public int col = 5;
    public List<ColorValue> colorTextList = new List<ColorValue>();
    public float colorThreshold = 50f;
    public float blackThreshold = 0.3f;
    //public Color customColor1, customColor2, customColor3, blank, pauseColor;
    public Vector2Int redRange_head;
    public Vector2Int redRange_end;
    public Vector2Int blueRange;
    public Vector2Int yellowRange;

    private ColorHSV gridHSV;
    public int redNumber = 0;
    public int maxRedToEnter = 3;

   //bool[,] overlappedGridCells;
    //float colorDifferent1;
    //float colorDifferent2;
    //float colorDifferent3;
    //float blankDifferent4;
    //float pauseDifferent;

    public float enterRedCheckDelays = 2f;
    public float leaveRedCheckDelays = 2f;
    public float delayToCheckDetection = 0f;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    public void resetData()
    {
        this.delayToCheckDetection = this.enterRedCheckDelays;
    }

  
    public void setWebcamSize(int _width, int _height)
    {
        this.width = _width;
        this.height = _height;
        Screen.SetResolution(this.width, this.height, true);
        this.canvasScaler.referenceResolution = new Vector2Int(this.width, this.height);
        this.croppedMat = new Mat();
        for (int i = 0; i < canvasRemindPoints.Length; i++)
        {
            setCanvasRemindPoints(showMarker, i);
        }

        // Create a new LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 3f;
        lineRenderer.endWidth = 3f;
        this.croppedTexture = new Texture2D(this.width, this.height, TextureFormat.RGBA32, false);
        this.webcamTexture = new Texture2D(this.width, this.height, TextureFormat.RGB24, false);
        this.renderTexture = new RenderTexture(this.width, this.height, 24);
        this.dstPoints = new MatOfPoint2f(
                new Point(0, 0),
                new Point(this.width, 0),
                new Point(this.width, this.height),
                new Point(0, this.height)
            );
        this.srcPoints = new MatOfPoint2f(
            new Point(topLeft.x + margin.x, topLeft.y - margin.y),
            new Point(topRight.x - margin.x, topRight.y - margin.y), 
            new Point(bottomRight.x - margin.x, bottomRight.y + margin.y),
            new Point(bottomLeft.x + margin.x, bottomLeft.y + margin.y)
        );
        this.readRect = new UnityEngine.Rect(0, 0, this.width, this.height);
        this.originalCroppedImageGridLayout = this.croppedImage.GetComponent<GridLayoutGroup>();
        this.originalCroppedImageGridLayout.cellSize = new Vector2((this.width * this.halfScale.x) / this.col,
                                                                   (this.height * this.halfScale.x) / this.row);
        //this.customColor1HSV = ColorHSV.FromRGB(customColor1);
        //this.customColor2HSV = ColorHSV.FromRGB(customColor2);
        //this.customColor3HSV = ColorHSV.FromRGB(customColor3);

        this.cellWidth = croppedTexture.width / col;
        this.cellHeight = croppedTexture.height / row;
        this.gridHSV = new ColorHSV(0f, 0f, 0f);
        this.resetData();
        //overlappedGridCells = new bool[row, col];

    }

    private void Start()
    {
        if (ConfigPage.Instance == null) { 
            SceneManager.LoadScene(0);
            return;
        }
        else
        {
            this.saveImage.init();
            this.croppedImage.GetComponent<RectTransform>().localScale = this.halfScale;
            this.originalCroppedTexture = this.croppedImage.GetComponent<RectTransform>();
            this.originalCroppedPosition = this.originalCroppedTexture.anchoredPosition;
            this.originalCroppedSizeDelta = this.originalCroppedTexture.sizeDelta;

            for (int i = 0; i < markers.Length; i++)
            {
                Vector3 configPos = ConfigPage.Instance.configData.markersPosition[i].position;
                markers[i].marker.transform.localPosition = configPos;
            }
            this.blackThreshold = ConfigPage.Instance.configData.blackThreshold;
            this.maxRedToEnter = ConfigPage.Instance.configData.maxRed;
            this.isFullscreen = ConfigPage.Instance.configData.fullscreen;
            this.redRange_head = ConfigPage.Instance.configData.redRange_head;
            this.redRange_end = ConfigPage.Instance.configData.redRange_end;
            this.blueRange = ConfigPage.Instance.configData.blueRange;
            this.yellowRange = ConfigPage.Instance.configData.yellowRange;
            this.enterRedCheckDelays = ConfigPage.Instance.configData.enterRedCheckDelays;
            this.leaveRedCheckDelays = ConfigPage.Instance.configData.leaveRedCheckDelays;

            float width = AvProCamController.Instance.resolution.x;
            float height = AvProCamController.Instance.resolution.y;
            //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
            if (fpsMonitor != null)
            {
                fpsMonitor.Add("width", width.ToString());
                fpsMonitor.Add("height", height.ToString());
                fpsMonitor.Add("orientation", Screen.orientation.ToString());
            }

            Camera.main.orthographicSize = height;
            this.setWebcamSize((int)width, (int)height);

            if (this.isFullscreen)
            {
                // Make the image fullscreen to parent object
                originalCroppedTexture.anchorMin = Vector2.zero;
                originalCroppedTexture.anchorMax = Vector2.one;
                originalCroppedTexture.offsetMin = Vector2.zero;
                originalCroppedTexture.offsetMax = Vector2.zero;
                originalCroppedTexture.anchoredPosition = Vector2.zero;
                this.croppedImage.GetComponent<RectTransform>().localScale = Vector3.one;
                this.originalCroppedImageGridLayout.cellSize = new Vector2((this.width / this.col),
                                                                           (this.height / this.row));

                this.lineRenderer.enabled = false;
            }
        }
       

    }

    void setCanvasRemindPoints(bool show, int i)
    {
        if (this.canvasRemindPoints[i] != null) this.canvasRemindPoints[i].alpha = show ? 1f : 0f;
    }
   
    void Update()
    {
        if (AvProCamController.Instance == null)
            return;

        if (this.webcamRawImage != null)
            this.webcamRawImage.texture = AvProCamController.Instance.OutputTexture;

        if (markers.Length > 0)
        {
            this.blackThreshold = ConfigPage.Instance.configData.blackThreshold;
            for (int i = 0; i < markers.Length; i++)
            {
                if (markers[i].marker != null)
                {
                    Vector3 markerPos = markers[i].marker.transform.localPosition;
                    ConfigPage.Instance.configData.markersPosition[i].position = markerPos;
                    markers[i].pos = markerPos;
                    markers[i].canvasPos = targetCamera.WorldToScreenPoint(markers[i].pos);
                    markers[i].wrapPos.x = markers[i].canvasPos.x;
                    markers[i].wrapPos.y = (Screen.height - markers[i].canvasPos.y);
                    if (markers[i].marker_status != null)markers[i].marker_status.SetActive(showMarker);
                    //For Debug in inspector
                     switch (i)
                     {
                         case 0:
                             if (horizontalFlip)
                             {
                                 if (verticalFlip)
                                 {
                                     bottomRight = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     bottomLeft = markers[i].wrapPos;
                                 }
                             }
                             else
                             {                            
                                 if (verticalFlip)
                                 {
                                     topRight = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     topLeft = markers[i].wrapPos;
                                 }
                             }
                             break;
                         case 1:
                             if (horizontalFlip)
                             {
                                 if (verticalFlip)
                                 {
                                     bottomLeft = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     bottomRight = markers[i].wrapPos;
                                 }                             
                             }
                             else
                             {
                                 if (verticalFlip)
                                 {
                                     topLeft = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     topRight = markers[i].wrapPos;
                                 }
                                 break;
                             }
                             break;
                         case 2:
                             if (horizontalFlip)
                             {                             
                                 if (verticalFlip)
                                 {
                                     topRight = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     topLeft = markers[i].wrapPos;
                                 }
                             }
                             else
                             {
                                 if (verticalFlip)
                                 {
                                     bottomRight = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     bottomLeft = markers[i].wrapPos;
                                 }
                             }
                             break;
                         case 3:
                             if (horizontalFlip)
                             {                              
                                 if (verticalFlip)
                                 {
                                     topLeft = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     topRight = markers[i].wrapPos;
                                 }
                             }
                             else
                             {
                                 if (verticalFlip)
                                 {
                                     bottomLeft = markers[i].wrapPos;
                                 }
                                 else
                                 {
                                     bottomRight = markers[i].wrapPos;
                                 }
                             }
                             break;
                     }
                }
            }

            // Draw lines between the corner points
            if (topLeft != null && topRight != null && bottomLeft != null && bottomRight != null)
            {
                // Set the number of points in the line renderer
                lineRenderer.positionCount = 5;
                // Set the positions of the line renderer points
                lineRenderer.SetPosition(0, markers[0].pos);
                lineRenderer.SetPosition(1, markers[1].pos);
                lineRenderer.SetPosition(2, markers[3].pos);
                lineRenderer.SetPosition(3, markers[2].pos);
                lineRenderer.SetPosition(4, markers[0].pos); 
            }
            else
            {
                // If the corner points are not set, disable the line renderer
                lineRenderer.positionCount = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            checkFullScreen(true);
        }

        this.croppedImage.texture = GetCroppedTexture;

        if(!GameController.Instance.IsAnyVideoPlaying())
        {
            if (GameController.Instance.CurrentPageId == 0)
            {
                if (RedNumber >= this.maxRedToEnter)
                {
                    if (this.delayToCheckDetection > 0)
                    {
                        this.delayToCheckDetection -= Time.deltaTime;
                    }
                    else
                    {
                        GameController.Instance.checkDetection(true);
                        this.delayToCheckDetection = this.enterRedCheckDelays;
                    }
                }
                else
                {
                    this.delayToCheckDetection = this.enterRedCheckDelays;
                }
            }
            else if (GameController.Instance.CurrentPageId == 2)
            {
                if (RedNumber == 0)
                {
                    if (this.delayToCheckDetection > 0)
                    {
                        this.delayToCheckDetection -= Time.deltaTime;
                    }
                    else
                    {
                        GameController.Instance.checkDetection(false);
                        this.delayToCheckDetection = this.leaveRedCheckDelays;
                    }
                }
                else
                {
                    this.delayToCheckDetection = this.leaveRedCheckDelays;
                }
            }
            else
            {
                this.delayToCheckDetection = this.enterRedCheckDelays;
            }
        }       
      
        
        if (Input.anyKeyDown)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(kcode))
                {
                    Debug.Log("keyCode down: " + kcode);
                    break;
                }
            }

        }

    }


    void checkFullScreen(bool switchScreen)
    {
        if (isFullscreen)
        {
            // Return to original size and position
            originalCroppedTexture.anchorMin = new Vector2(0f, 1f);
            originalCroppedTexture.anchorMax = new Vector2(0f, 1f);
            originalCroppedTexture.offsetMin = Vector3.one;
            originalCroppedTexture.offsetMax = Vector3.one;
            originalCroppedTexture.anchoredPosition = originalCroppedPosition;
            originalCroppedTexture.sizeDelta = originalCroppedSizeDelta;
            this.croppedImage.GetComponent<RectTransform>().localScale = this.halfScale;
            this.originalCroppedImageGridLayout.cellSize = new Vector2((this.width * this.halfScale.x) / this.col,
                                                                       (this.height * this.halfScale.x) / this.row);
            this.lineRenderer.enabled = true;
        }
        else
        {
            // Make the image fullscreen to parent object
            originalCroppedTexture.anchorMin = Vector2.zero;
            originalCroppedTexture.anchorMax = Vector2.one;
            originalCroppedTexture.offsetMin = Vector2.zero;
            originalCroppedTexture.offsetMax = Vector2.zero;
            originalCroppedTexture.anchoredPosition = Vector2.zero;
            this.croppedImage.GetComponent<RectTransform>().localScale = Vector3.one;
            this.originalCroppedImageGridLayout.cellSize = new Vector2((this.width / this.col),
                                                                       (this.height / this.row));

            this.lineRenderer.enabled = false;
        }

        if(switchScreen)
            isFullscreen = !isFullscreen;

       ConfigPage.Instance.configData.fullscreen = isFullscreen;
       ConfigPage.Instance.Save();
    }
    void capture()
    {
        if (croppedImage != null)
        {
            croppedImage.GetComponent<RectTransform>().sizeDelta = new Vector2(GetCroppedTexture.width * 0.15f, GetCroppedTexture.height * 0.15f);
            croppedImage.texture = GetCroppedTexture;           
        }
        saveImage.SavePhotoToLocal(GetCroppedTexture);
    }

    public Mat webcamCapture()
    {
        RenderTexture.active = this.renderTexture;
        this.targetCamera.targetTexture = this.renderTexture;
        this.targetCamera.Render();
        RenderTexture.active = this.renderTexture;
        this.webcamTexture.ReadPixels(this.readRect, 0, 0);
        this.webcamTexture.Apply();
        Utils.texture2DToMat(this.webcamTexture, this.WebcamMat);
        this.targetCamera.targetTexture = null;
        return this.WebcamMat;
    }


    public Mat WebcamMat
    {
        get { 
            Mat webcamMat = AvProCamController.Instance.GetMat;
            return webcamMat;
        }
    }

    float ColorDiff(Color color1, Color color2)
    {
        float diffR = Mathf.Abs(color1.r - color2.r);
        float diffG = Mathf.Abs(color1.g - color2.g);
        float diffB = Mathf.Abs(color1.b - color2.b);
        return (diffR + diffG + diffB) / 3f;
    }

    int ColorDiffHSVType(ColorHSV color1)
    {
        int type = 0;
        if (color1.v <= this.blackThreshold)
        {
            type = 0;
        }
        else
        {
            if(color1.h >= 0f && color1.h < 60f ||
               color1.h >= 300f && color1.h <= 360)
            {
                type = 1;
            }

            if (color1.h >= 60f && color1.h < 120f)
            {
                type = 2;
            }

            if (color1.h >= 180f && color1.h < 240f)
            {
                type = 3;
            }
        }

        return type;
    }


    int ColorIdentify(ColorHSV _color)
    {
        int value = 0;

        if(_color.v <= this.blackThreshold)
        {
            value = 0;
        }
        else
        {
            if ((_color.h > redRange_head.x && _color.h < redRange_head.y) || 
                (_color.h >= redRange_end.x && _color.h < redRange_end.y))
            {
                value = 1;
            }

            if (_color.h >= blueRange.x && _color.h < blueRange.y)
            {
                value = 2;
            }

            if (_color.h >= yellowRange.x && _color.h < yellowRange.y)
            {
                value = 3;
            }
        }

        return value;
    }

    float ColorDiffHSV(ColorHSV color1, ColorHSV color2)
    {

        if (color1.v <= this.blackThreshold)
        {
            return 0f; 
        }
        else
        {
            float hueDiff = Mathf.Abs(color1.h - color2.h);
            float saturationDiff = Mathf.Abs(color1.s - color2.s);
            float valueDiff = Mathf.Abs(color1.v - color2.v);

            // Normalize hue difference
            if (hueDiff > 180f)
            {
                hueDiff = 360f - hueDiff;
            }

            // Calculate overall difference
            float diff = Mathf.Sqrt(Mathf.Pow(hueDiff, 2) + Mathf.Pow(saturationDiff, 2) + Mathf.Pow(valueDiff, 2));
            return diff;
        }
    }

    public int RedNumber
    {
        get
        {
            return this.redNumber;
        }
        set
        {
            this.redNumber = value;
        }
    }

    void updateRedNumber(bool allow = true)
    {
        if (GameController.Instance.IsDetectionPages()) { 
            if (allow)
            {
                this.RedNumber += 1;
            }
            else
            {
                this.RedNumber = 0;
            }
        }
    }

    public Texture2D GetCroppedTexture
    {
        get
        {
            updateRedNumber(false);

            this.srcPoints.fromArray(
                new Point(topLeft.x + margin.x, topLeft.y - margin.y),
                new Point(topRight.x - margin.x, topRight.y - margin.y),
                new Point(bottomRight.x - margin.x, bottomRight.y + margin.y),
                new Point(bottomLeft.x + margin.x, bottomLeft.y + margin.y)
            );

            Mat perspectiveTransform = Imgproc.getPerspectiveTransform(this.srcPoints, this.dstPoints);
            Imgproc.warpPerspective(this.WebcamMat, croppedMat, perspectiveTransform, this.croppedMat.size());
            Utils.matToTexture2D(this.croppedMat, this.croppedTexture);

            Color32[] pixels = this.croppedTexture.GetPixels32();
            //string colorText;

            if(useGridDetect)
            {
                StringBuilder sb = new StringBuilder();
                // Iterate over the grid cells
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        int startX = j * this.cellWidth;
                        int startY = (row - i - 1) * this.cellHeight;
                        int endX = startX + this.cellWidth;
                        int endY = startY + this.cellHeight;
                        sb.Clear();
                        sb.Append(i + "_" + j);

                        float hSum = 0f;
                        float sSum = 0f;
                        float vSum = 0f;
                        int pixelCount = 0;

                        for (int x = startX; x < endX; x++)
                        {
                            for (int y = startY; y < endY; y++)
                            {
                                if (x < this.width && y < this.height)
                                {
                                    int index = (this.height - y - 1) * this.width + x;
                                    Color32 pixel = pixels[index];
                                    ColorHSV pixelHSV = ColorHSV.FromRGB(pixel.r, pixel.g, pixel.b);
                                    hSum += pixelHSV.h;
                                    sSum += pixelHSV.s;
                                    vSum += pixelHSV.v;
                                    pixelCount++;
                                }
                            }
                        }

                        float hAvg = hSum / pixelCount;
                        float sAvg = sSum / pixelCount;
                        float vAvg = vSum / pixelCount;
                        //this.blankHSV = ColorHSV.FromRGB(blank);
                        //this.pauseHSV = ColorHSV.FromRGB(pauseColor);
                        this.gridHSV.H = hAvg;
                        this.gridHSV.S = sAvg;
                        this.gridHSV.V = vAvg;

                        int type = ColorIdentify(this.gridHSV);

                        switch (type)
                        {
                            case 0:
                                sb.Append("_Blank");
                                //overlappedGridCells[i, j] = false;
                                break;
                            case 1:
                                //Debug.Log("colorDifferent1: " + colorDifferent1);
                                sb.Append("_Red");
                                //overlappedGridCells[i, j] = false;
                                updateRedNumber(true);
                                break;
                            case 2:
                                //Debug.Log("colorDifferent2: " + colorDifferent2);
                                sb.Append("_Blue");
                                //overlappedGridCells[i, j] = false;
                                break;
                            case 3:
                                // Debug.Log("colorDifferent3: " + colorDifferent3);
                                sb.Append("_Yellow");
                                //overlappedGridCells[i, j] = false;
                                break;
                        }

                        /*colorDifferent1 = ColorDiffHSV(new ColorHSV(hAvg, sAvg, vAvg), customColor1HSV);
                        colorDifferent2 = ColorDiffHSV(new ColorHSV(hAvg, sAvg, vAvg), customColor2HSV);
                        colorDifferent3 = ColorDiffHSV(new ColorHSV(hAvg, sAvg, vAvg), customColor3HSV);
                        blankDifferent4 = ColorDiffHSV(new ColorHSV(hAvg, sAvg, vAvg), blankHSV);
                        //pauseDifferent = ColorDiffHSV(new ColorHSV(hAvg, sAvg, vAvg), pauseHSV);

                        if (blankDifferent4 == 0)
                        {
                            sb.Append("_Blank");
                            //overlappedGridCells[i, j] = false;
                        }
                        else
                        {
                            float minDifference = Mathf.Min(colorDifferent1, colorDifferent2, colorDifferent3);
                            if (minDifference == colorDifferent1)
                            {
                                //Debug.Log("colorDifferent1: " + colorDifferent1);
                                sb.Append("_Red");
                                //overlappedGridCells[i, j] = false;
                                updateRedNumber(true);
                            }
                            else if (minDifference == colorDifferent2)
                            {
                                //Debug.Log("colorDifferent2: " + colorDifferent2);
                                sb.Append("_Blue");
                                //overlappedGridCells[i, j] = false;
                            }
                            else if (minDifference == colorDifferent3)
                            {
                                // Debug.Log("colorDifferent3: " + colorDifferent3);
                                sb.Append("_Yellow");
                                //overlappedGridCells[i, j] = false;
                            }
                            else
                            {
                                sb.Append("_Blank");
                                // overlappedGridCells[i, j] = false;
                            }
                        }*/

                        // Draw the grid lines
                        for (int x = startX; x < endX; x++)
                        {
                            croppedTexture.SetPixel(x, startY, Color.black);
                            croppedTexture.SetPixel(x, endY - 1, Color.black);
                        }
                        for (int y = startY; y < endY; y++)
                        {
                            croppedTexture.SetPixel(startX, y, Color.black);
                            croppedTexture.SetPixel(endX - 1, y, Color.black);
                        }

                        /* if (overlappedGridCells[i, j])
                         {
                             continue;
                         }*/

                        int id = i * col + j;
                        if (id < this.colorTextList.Count)
                        {
                            this.colorTextList[id].setHSVValue(sb.ToString(), gridHSV.H, gridHSV.S, gridHSV.V);
                        }
                        else if (this.colorTextList.Count < (row * col))
                        {
                            GameObject colorTextUI = Instantiate(colorTextPrefab, parent);
                            colorTextUI.GetComponent<ColorValue>().setHSVValue(sb.ToString(), gridHSV.H, gridHSV.S, gridHSV.V);
                            colorTextUI.name = "color_" + i + "_" + j;
                            this.colorTextList.Add(colorTextUI.GetComponent<ColorValue>());
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color32 pixel = pixels[i];
                    float h, s, v;
                    Color.RGBToHSV(pixel, out h, out s, out v);

                    // Check if the hue value falls within the desired range for red
                    if ((h >= 0f && h <= 0.05f) || (h >= 0.95f && h <= 1f))
                    {
                        // Pixel contains red color in the HSV color space
                        Debug.Log("Red detect!!");
                        break; // Exit the loop if red color is found
                    }
                }

            }
            

            croppedTexture.Apply();
            return croppedTexture;
        }
    }


}


[Serializable]
public class Corner
{
    public corners corner = corners.none;
    public Transform marker;
    public GameObject marker_status;
    public Vector3 pos;
    public Vector2 canvasPos;
    public Vector2 wrapPos;

    public enum corners
    {
        none,
        top_left,
        top_right,
        bottom_left,
        bottom_right
    }

}
[Serializable]
public struct ColorHSV
{
    public float h; // Hue value (0-360 degrees)
    public float s; // Saturation value (0-1)
    public float v; // Value/Brightness value (0-1)

    public float H
    {
        get { return h; }
        set { h = value; }
    }

    public float S
    {
        get { return s; }
        set { s = value; }
    }

    public float V
    {
        get { return v; }
        set { v = value; }
    }

    public ColorHSV(float h, float s, float v)
    {
        this.h = h;
        this.s = s;
        this.v = v;
    }

    public static ColorHSV FromRGB(byte r, byte g, byte b)
    {
        Color color = new Color32(r, g, b, 255);
        return FromRGB(color);
    }

    public static ColorHSV FromRGB(Color color)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return new ColorHSV(h * 180f, s, v);
    }

    public static ColorHSV OpencvRGBToHSV(Color color)
    {
        // Convert RGB color to OpenCV Mat
        Mat inputMat = new Mat(1, 1, CvType.CV_8UC3);
        inputMat.put(0, 0, new byte[] { (byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255) });

        // Convert RGB to HSV
        Mat hsvMat = new Mat();
        Imgproc.cvtColor(inputMat, hsvMat, Imgproc.COLOR_RGB2HSV);

        // Extract HSV values
        double h = hsvMat.get(0, 0)[0];
        double s = hsvMat.get(0, 0)[1];
        double v = hsvMat.get(0, 0)[2];

        // Create and return ColorHSV object
        return new ColorHSV((float)h, (float)s, (float)v);
    }


}

[System.Serializable]
public class SaveImage
{
    public OutputFormat outputFormat = OutputFormat.jpg;
    private long photoFormat;
    private string filepath;

    public void init()
    {
        if (!Directory.Exists(this.TempFolder))
            Directory.CreateDirectory(this.TempFolder);

        this.filepath = this.CaptureFolderPath("Images");

        if (!Directory.Exists(this.filepath))
            Directory.CreateDirectory(this.filepath);
    }

    private string TempFolder
    {
        get
        {
            return Directory.GetCurrentDirectory() + "/Capture/";
        }
    }

    private string DateFolderFormat
    {
        get
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }
    }

    public string CaptureFolderPath(string folderName)
    {
        return this.TempFolder + this.DateFolderFormat + "/" + folderName + "/";
    }

    public enum OutputFormat
    {
        jpg,
        png
    }

    private string CaptureOutPutFormat
    {
        get
        {
            string format = "";
            switch (outputFormat)
            {
                case OutputFormat.jpg:
                    format = ".jpg";
                    break;
                case OutputFormat.png:
                    format = ".png";
                    break;
            }
            return format;
        }
    }

    private byte[] CaptureOutPutBytes(Texture2D texture)
    {
        byte[] bytes = null;
        switch (outputFormat)
        {
            case OutputFormat.jpg:
                bytes = texture.EncodeToJPG();
                break;
            case OutputFormat.png:
                bytes = texture.EncodeToPNG();
                break;
        }
        return bytes;
    }
    public void SavePhotoToLocal(Texture2D screenShot)
    {
        // Loacl storage image
        if (Directory.Exists(filepath))
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            photoFormat = (long)(DateTime.UtcNow - epochStart).TotalSeconds;
            var path = filepath + "cp_" + photoFormat + this.CaptureOutPutFormat;
            File.WriteAllBytes(path, CaptureOutPutBytes(screenShot));

        }
    }
}

[Serializable]
public class HandDetection
{
    HandPoseEstimationExample.PalmDetector palmDetector;
    HandPoseEstimationExample.HandPoseEstimator handPoseEstimator;
    protected static readonly string PALM_DETECTION_MODEL_FILENAME = "OpenCVForUnity/dnn/palm_detection_mediapipe_2023feb.onnx";
    string palm_detection_model_filepath;
    protected static readonly string HANDPOSE_ESTIMATION_MODEL_FILENAME = "OpenCVForUnity/dnn/handpose_estimation_mediapipe_2023feb.onnx";
    string handpose_estimation_model_filepath;
    public List<float[]> TriggerHands = null;
    float[] bbox = new float[4];
    Mat bgrMat;

    public void Init(Mat webCamTextureMat)
    {
        this.TriggerHands = new List<float[]>();
        bgrMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);
        palm_detection_model_filepath = Utils.getFilePath(PALM_DETECTION_MODEL_FILENAME);
        handpose_estimation_model_filepath = Utils.getFilePath(HANDPOSE_ESTIMATION_MODEL_FILENAME);
        if (string.IsNullOrEmpty(palm_detection_model_filepath))
        {
            Debug.LogError(PALM_DETECTION_MODEL_FILENAME + " is not loaded. Please read “StreamingAssets/OpenCVForUnity/dnn/setup_dnn_module.pdf” to make the necessary setup.");
        }
        else
        {
            palmDetector = new HandPoseEstimationExample.PalmDetector(palm_detection_model_filepath, 0.3f, 0.6f);
        }

        if (string.IsNullOrEmpty(handpose_estimation_model_filepath))
        {
            Debug.LogError(HANDPOSE_ESTIMATION_MODEL_FILENAME + " is not loaded. Please read “StreamingAssets/OpenCVForUnity/dnn/setup_dnn_module.pdf” to make the necessary setup.");
        }
        else
        {
            handPoseEstimator = new HandPoseEstimationExample.HandPoseEstimator(handpose_estimation_model_filepath, 0.9f);
        }
        
    }

    private List<float[]> handDetect(Mat rgbaMat, Texture2D texture)
    {
        this.TriggerHands.Clear();
        //Debug.Log("start trigger hand");
        if (palmDetector == null || handPoseEstimator == null)
        {
            Imgproc.putText(rgbaMat, "model file is not loaded.", new Point(5, rgbaMat.rows() - 30), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
            Imgproc.putText(rgbaMat, "Please read console message.", new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.7, new Scalar(255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
        }
        else
        {
            Imgproc.cvtColor(rgbaMat, bgrMat, Imgproc.COLOR_RGBA2BGR);
            Mat palms = palmDetector.infer(bgrMat);
            List<Mat> hands = new List<Mat>();

            // Estimate the pose of each hand
            for (int i = 0; i < palms.rows(); ++i)
            {
                // Handpose estimator inference
                Mat hodpose = handPoseEstimator.infer(bgrMat, palms.row(i));
                if (!hodpose.empty())
                    hands.Add(hodpose);
            }

            Imgproc.cvtColor(bgrMat, rgbaMat, Imgproc.COLOR_BGR2RGBA);

            //palmDetector.visualize(rgbaMat, palms, false, true);
            handPoseEstimator.visualize(rgbaMat, hands, false, true);
            for (int i = 0; i < hands.Count; i++)
            {
                if (hands[i] != null)
                {
                    hands[i].get(0, 0, this.bbox);
                }
                this.TriggerHands.Add(this.bbox);
            }
        }

        Utils.matToTexture2D(rgbaMat, texture);
        return this.TriggerHands;
    }


}

