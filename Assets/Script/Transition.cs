using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{

    public Animator transitionAnim;
    public string SceneName;

    public void OnMouseDown()
    {
        if(SceneName == "Quiz")
            PlayMusicMenu.instance.gameObject.GetComponent<AudioSource>().Stop();
        if (!transitionAnim)
        {
            SceneManager.LoadScene(SceneName);
        }
        else
        {
            StartCoroutine(LoadScene());
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("ViewLessonSCene");
    }

    IEnumerator LoadScene()
    {
        transitionAnim.SetTrigger("end");
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(SceneName);
    }
}
