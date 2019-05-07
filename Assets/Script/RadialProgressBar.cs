using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialProgressBar : MonoBehaviour
{

    public Transform LoadingBar;
    public Text TextIndicator;


    // Update is called once per frame
    void Update()
    {
        var TotalScoreProgress = PlayerPrefs.GetInt(GameUtility.ProgessTotalScore);
        
        float f = TotalScoreProgress;
        Debug.Log(TotalScoreProgress);

        TextIndicator.text = (f/240).ToString("F2") + "%";
        LoadingBar.GetComponent<Image>().fillAmount = (f/24000);   
    }
}
