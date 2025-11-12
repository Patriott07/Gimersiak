using UnityEngine;
using UnityEngine.Video;

public class Cutscene : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.playOnAwake = false;  // biar gak auto play
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("✅ Video siap diputar!");
        vp.Play();
    }
}
