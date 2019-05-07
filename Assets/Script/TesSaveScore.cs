using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TesSaveScore : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Set Current Scene to LastStageLoad -> will get by GameManager
        PlayerPrefs.SetString(GameUtility.LastStageLoad, SceneManager.GetActiveScene().name);
        Debug.Log(PlayerPrefs.GetString(GameUtility.LastStageLoad));

        //StartCoroutine(Timer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("ViewLessonSCene");
    }
}
