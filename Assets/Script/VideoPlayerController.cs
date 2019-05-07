using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class VideoPlayerController : MonoBehaviour
{
    public RawImage image;
    public VideoPlayer videoPlayer;
    IEnumerator playVideo()
    {
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }
        image.texture = videoPlayer.texture;
        videoPlayer.Play();
    }
    public void PlayPause()
    {
            StartCoroutine(playVideo());
    }
}
