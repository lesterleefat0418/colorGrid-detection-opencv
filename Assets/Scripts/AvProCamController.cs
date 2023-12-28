using UnityEngine.Scripting;
using UnityEngine;
using RenderHeads.Media.AVProLiveCamera;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;

public class AvProCamController : MonoBehaviour
{
    public static AvProCamController Instance = null;
    public Vector2Int resolution;
    [SerializeField] private AVProLiveCamera liveCamera;
    public bool isWebcamStarted = false;
    Mat webcamMat;
    private Texture2D tex2D = null;
    private UnityEngine.Rect rect;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {       
        // Open Webcam
        this.ToggleCamera();

    }

    public Texture OutputTexture
    {
        get
        {
            return this.liveCamera.OutputTexture;
        }
    }

    public Texture2D OutputTexture2D
    {
        get
        {
            return TextureToTexture2D(this.liveCamera.OutputTexture);
        }
    }


    public Mat GetMat
    {
        get
        {
            if(OutputTexture != null)
                Utils.texture2DToMat(TextureToTexture2D(OutputTexture), webcamMat);

            return webcamMat;
        }
    }

    private Texture2D TextureToTexture2D(Texture texture)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);
        RenderTexture.active = renderTexture;
        this.tex2D.ReadPixels(this.rect, 0, 0);
        this.tex2D.Apply();
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        return this.tex2D;
    }


    // Invoked by Unity Event
    [Preserve]
    public void ToggleCamera()
    {
        if (!isWebcamStarted)
            StartCameras();
    }

    private void StartCameras()
    {
        this.liveCamera.Begin();
        isWebcamStarted = true;
        this.webcamMat = new Mat(this.resolution.y, this.resolution.x, CvType.CV_8UC4);
        this.tex2D = new Texture2D(this.resolution.x, this.resolution.y, TextureFormat.RGBA32, false);
        this.rect = new UnityEngine.Rect(0, 0, this.resolution.x, this.resolution.y);
    }

}

