using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ViewLessonManager : MonoBehaviour
{
    [System.Serializable]
    public class Level
    {
        public string LevelText;
        public string CategoryText;
        public int Unlocked;
        public bool IsInteractable;

        public Button.ButtonClickedEvent onClickedEvent;
    }

    public GameObject levelButton;
    public Transform LessonButtonContent;
    public Text TotalProgressScore;
    public List<Level> LevelList;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        FillList();
        TotalProgressScore.text = PlayerPrefs.GetInt(GameUtility.ProgessTotalScore).ToString() + " / 24000";
        //AudioManager.Instance.PlaySound("MenuMusic");
        //PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower", 0);
        //Debug.Log("Tokyo Tower Score: " + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower"));
    }
    
    void FillList()
    {
        foreach (var level in LevelList)
        {
            GameObject newButton = Instantiate(levelButton) as GameObject;
            ViewLessonButton button = newButton.GetComponent<ViewLessonButton>();
            
            button.LevelText.text = level.LevelText;
            button.CategoryLessonText.text = level.CategoryText;
            button.unlocked = level.Unlocked;

            //Player Prefab to check if the series is unlocked or not
            //LevelA Series, LevelK Series
            if (PlayerPrefs.GetInt(GameUtility.LevelUnlock + button.LevelText.text) == 1)
            {
                level.Unlocked = 1;
                level.IsInteractable = true;
            }

            if(PlayerPrefs.GetInt(GameUtility.SavePrefKey + button.LevelText.text) >= 1000 )
            {
                button.Star1.SetActive(true);
            }
            if(PlayerPrefs.GetInt(GameUtility.SavePrefKey + button.LevelText.text) >= 1500)
            {
                button.Star2.SetActive(true);
            }
            if(PlayerPrefs.GetInt(GameUtility.SavePrefKey + button.LevelText.text) >= 2000)
            {
                button.Star3.SetActive(true);
            }
            button.unlocked = level.Unlocked;
            button.GetComponent<Button>().interactable = level.IsInteractable;
            button.GetComponent<Button>().onClick.AddListener(() => LoadLevels(button.LevelText.text));
            
            newButton.transform.SetParent(LessonButtonContent, false);
        }
        SaveAll();
    }
    void SaveAll()
    {
        //if(PlayerPrefs.HasKey(GameUtility.LevelUnlock+"A Series"))
        //{
        //    return;
        //}
        //else
        {
            GameObject[] allButtons = GameObject.FindGameObjectsWithTag("LevelButton");
            foreach (GameObject buttons in allButtons)
            {
                ViewLessonButton button = buttons.GetComponent<ViewLessonButton>();
                PlayerPrefs.SetInt(GameUtility.LevelUnlock + button.LevelText.text, button.unlocked);
            }
        }
        
    }

    //Emergency untuk delete semua prefabs, jadi rset semua dari awal
    void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    void LoadLevels(string value)
    {
        Application.LoadLevel(value);
    } 
}
