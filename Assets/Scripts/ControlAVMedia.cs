using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;

public class ControlAVMedia : MonoBehaviour
{
    public DisplayUGUI display;
    public MediaPlayer[] mediaPlayer;
    public CanvasGroup[] textImages;
    public DisplayUGUI playerGUI;
    public bool isloop = false;
    public bool useRandomPlay = false;
    public int nextPageId;
    public bool _isPlaying =false;

    public bool IsPlaying
    {
        get
        {
            return this._isPlaying;
        }
        set
        {
            this._isPlaying = value;
        }
    }

    void Start()
    {
        if (mediaPlayer == null && mediaPlayer.Length == 0)
           return;

        this.openVideoFile();

        if(useRandomPlay) showEndTitle(-1);
    }

    public void openVideoFile()
    {
        foreach(var media in this.mediaPlayer)
        {
            media.m_Loop = isloop;
            if (string.IsNullOrEmpty(media.m_VideoPath))
            {
                media.CloseVideo();
            }
            else
            {
                media.OpenVideoFromFile(media.m_VideoLocation, media.m_VideoPath, false);
            }
        }
    }

    void showEndTitle(int targetId)
    {
        for (int i = 0; i < this.textImages.Length; i++)
        {
            if (this.textImages[i] != null)
            {
                if (i == targetId)
                {
                    this.textImages[i].alpha = 1f;
                }
                else
                {
                    this.textImages[i].alpha = 0f;
                }
            }
        }
    }

    public void playVideo(int defaultVideo=0)
    {
        int randVideoId = useRandomPlay ? Random.Range(0, this.mediaPlayer.Length) : defaultVideo;

        if(useRandomPlay)
        {
            this.showEndTitle(randVideoId);
        }

        if (this.mediaPlayer[randVideoId] != null)
        {
            this.display._mediaPlayer = this.mediaPlayer[randVideoId];
            this.mediaPlayer[randVideoId].Control.Rewind();
            this.mediaPlayer[randVideoId].Control.Play();
            this.IsPlaying = true;
            Debug.Log("Play video");
        }
    }


    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        foreach (var mediaPlayer in mediaPlayer)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
            }
        }
    }

    private void UnsubscribeFromEvents()
    {
        foreach (var mediaPlayer in mediaPlayer)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Events.RemoveListener(OnMediaPlayerEvent);
            }
        }
    }

    private void OnMediaPlayerEvent(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
    {
        if (eventType == MediaPlayerEvent.EventType.FinishedPlaying && this.IsPlaying)
        {
            // Video has finished playing
            Debug.Log("Video finished playing");

            if(GameController.Instance != null)
               GameController.Instance.changePage(this.nextPageId);

            this.IsPlaying = false;
            this.rewindVideo();

        }
    }

    public void rewindVideo()
    {
        foreach (var mediaPlayer in mediaPlayer)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Control.Rewind();
            }
        }
    }

    void pauseVideo(int videoId)
    {
        if (this.mediaPlayer != null)
        {
            this.mediaPlayer[videoId].Control.Pause();
        }
    }

    public void stopVideo(int videoId)
    {
        if (this.mediaPlayer != null)
        {
            this. mediaPlayer[videoId].Control.Rewind();
            this.mediaPlayer[videoId].Control.SeekFast((0f) * 1000f);
            this.mediaPlayer[videoId].Control.Stop();
        }
    }


}
