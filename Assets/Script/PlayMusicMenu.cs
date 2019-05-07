using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusicMenu : MonoBehaviour
{
    private static PlayMusicMenu Instance;
    public static PlayMusicMenu instance { get { return Instance; } }
    
    void Awake()
    {
        if ( Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }
}
