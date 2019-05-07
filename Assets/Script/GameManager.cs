using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System.Xml;

public class GameManager : MonoBehaviour
{
    private Data data = new Data();

    [SerializeField] GameEvents events = null;

    [SerializeField] Animator timerAnimator = null;

    [SerializeField] Text timerText = null;

    [SerializeField] Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] Color timerAlmostOutColor = Color.red;

    private Color timerDefaultColor = Color.white;
    private List<AnswersData> PickedAnswer = new List<AnswersData>();

    private List<int> FinishedQuestions = new List<int>();
    private int currentQuestion = 0;

    private int timerStateParaHash = 0;

    private IEnumerator IE_WaitTillNextRound = null;
    private IEnumerator IE_StartTimer = null;

    string[] paths;

    private bool IsFinished
    {
        get
        {
            return(FinishedQuestions.Count < data.Questions.Length) ? false : true;
        }
    }

    private void OnEnable()
    {
        events.UpdateQuestionAnswer += UpdateAnswer;
    }

    private void OnDisable()
    {
        events.UpdateQuestionAnswer -= UpdateAnswer;
    }

    private void Awake()
    {
        events.CurrentFinalScore = 0;
        BetterStreamingAssets.Initialize();
        paths = BetterStreamingAssets.GetFiles("\\", "*.xml", SearchOption.AllDirectories);
        
    }

    void Start()
    {
        for (int i = 0; i < paths.Length; i++)
        {
            Debug.Log(paths[i]);
        }
        events.StartupHighscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey + GameUtility.LastStageLoad);
        Debug.Log(PlayerPrefs.GetString(GameUtility.LastStageLoad));
        timerDefaultColor = timerText.color;
        LoadData();        

        timerStateParaHash = Animator.StringToHash("TimerState");

        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        Display();
    }

    public void UpdateAnswer(AnswersData newAnswer)
    {
        if(data.Questions[currentQuestion].Type == AnswerType.Single)
        {
            foreach (var answer in PickedAnswer)
            {
                if (answer != newAnswer)
                {
                    answer.Reset();
                }
            }
            PickedAnswer.Clear();
            PickedAnswer.Add(newAnswer);
        }
        else
        {
            bool alreadyPicked = PickedAnswer.Exists(x => x == newAnswer);
            if (alreadyPicked)
            {
                PickedAnswer.Remove(newAnswer);
            }
            else
            {
                PickedAnswer.Add(newAnswer);
            }
        }
    }

    public void EraseAnswer()
    {
        PickedAnswer = new List<AnswersData>();
    }

    void Display()
    {
        EraseAnswer();
        var question = GetRandomQuestion();

        if (events.UpdateQuestionUI != null)
        {
            events.UpdateQuestionUI(question);
        }
        else
        {
            Debug.LogWarning("Ups! something went wrong while trying to display new question UI Data. GameEvents.UpdateQuestionUI is null. Issue occured in GameManager.Display() Method");
        }

        if (question.UseTimer)
        {
            UpdateTimer(question.UseTimer);
        }
    }

    public void Accept()
    {
        UpdateTimer(false);
        bool isCorrect = CheckAnswer();
        FinishedQuestions.Add(currentQuestion);
        
        UpdateScore((isCorrect) ? data.Questions[currentQuestion].AddScore : 0);

        if (IsFinished)
        {
            SetHighscore();
        }

        var type = (IsFinished) ? UIManager.ResolutionScreenType.Finish : (isCorrect) ? UIManager.ResolutionScreenType.Correct : UIManager.ResolutionScreenType.Incorrect;

        if (events.DisplayResolutionScreen != null)
        {
            events.DisplayResolutionScreen(type, data.Questions[currentQuestion].AddScore);
        }

        AudioManager.Instance.PlaySound((isCorrect) ? "CorrectSFX" : "IncorrectSFX");

        if(type != UIManager.ResolutionScreenType.Finish)
        {
            if (IE_WaitTillNextRound != null)
            {
                StopCoroutine(IE_WaitTillNextRound);
            }
            IE_WaitTillNextRound = WaitTillNextRound();
            StartCoroutine(IE_WaitTillNextRound);
        }

        
    }

    void UpdateTimer (bool state)
    {
        switch (state)
        {
            case true:
                IE_StartTimer = StartTimer();
                StartCoroutine(IE_StartTimer);

                timerAnimator.SetInteger(timerStateParaHash, 2);
                break;
            case false:
                if(IE_StartTimer != null)
                {
                    StopCoroutine(IE_StartTimer);
                }
                timerAnimator.SetInteger(timerStateParaHash, 1);
                break;
        }
    }

    IEnumerator StartTimer()
    {
        var totalTime = data.Questions[currentQuestion].Timer;
        var timeLeft = totalTime;

        timerText.color = timerDefaultColor;
        while (timeLeft > 0)
        {
            timeLeft--;

            AudioManager.Instance.PlaySound("CountdownSFX");

            if(timeLeft < totalTime / 2 && timeLeft > totalTime / 4)
            {
                timerText.color = timerHalfWayOutColor;
            }
            if(timeLeft < totalTime / 4)
            {
                timerText.color = timerAlmostOutColor;
            }

            timerText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1.0f);
        }
        //Kalau waktu abis terserah mau diapain
        Accept();
    }

    IEnumerator WaitTillNextRound()
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        Display();
    }

    Question GetRandomQuestion()
    {
        var randomIndex = GetRandomQuestionIndex();
        currentQuestion = randomIndex;

        return data.Questions[currentQuestion];
    }

    int GetRandomQuestionIndex()
    {
        var random = 0;
        if (FinishedQuestions.Count < data.Questions.Length)
        {
            do
            {
                random = UnityEngine.Random.Range(0, data.Questions.Length);
            } while (FinishedQuestions.Contains(random) || random == currentQuestion);
        }
        return random;
    }

    bool CheckAnswer()
    {
        if (!CompareAnswer())
        {
            return false;
        }
        return true;
    }
    bool CompareAnswer()
    {
        if (PickedAnswer.Count > 0)
        {
            List<int> c = data.Questions[currentQuestion].GetCorrectAnswers();
            List<int> p = PickedAnswer.Select(x => x.AnswerIndex).ToList();

            var f = c.Except(p).ToList();
            var s = p.Except(c).ToList();

            return !f.Any() && !s.Any();
        }
        return false;
    }

    //void LoadQuestion()
    //{
    //    Object[] objs = Resources.LoadAll("Questions", typeof(Question));
    //    _questions = new Question[objs.Length];
    //    for (int i = 0; i < objs.Length; i++)
    //    {
    //        _questions[i] = (Question)objs[i];
    //    }

    //}
    void LoadData()
    {
        events.level = 1;
        //events.level hrus di tembak dl biar dia ambil Q.xml sesuai levelnya
        //data = Data.Fetch(Path.Combine(GameUtility.fileDir,GameUtility.FileName + PlayerPrefs.GetString(GameUtility.LastStageLoad) + ".xml"));
        //Debug.Log(Path.Combine(GameUtility.fileDir, GameUtility.FileName + GameUtility.LastStageLoad + ".xml"));
        //TextAsset temp = Resources.Load("QSeri-A") as TextAsset;
        //XmlDocument _doc = new XmlDocument();
        //_doc.LoadXml(temp.text);
        //data = Data.Fetch(_doc);
        //var path = "jar:file://" + Application.dataPath + "!/assets/";
        //var path = Path.Combine(Application.streamingAssetsPath, "QSeri-A.xml");
        //Debug.Log(path);
        //data = Data.Fetch(path);
        data = Data.ReadFromXml("QSeri-A.xml");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitGame()
    {
        AudioManager.Instance.StopAllSound();
        SceneManager.LoadScene("ViewLessonSCene");
    }

    private void SetHighscore()
    {
        var lastStageLoad = PlayerPrefs.GetString(GameUtility.LastStageLoad);
        var highscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey + lastStageLoad); //Ambil Score SavePrefKeySeri-*
        if (highscore < events.CurrentFinalScore)
        {
            PlayerPrefs.SetInt(GameUtility.SavePrefKey + lastStageLoad, events.CurrentFinalScore); //Set Score terbaru ke SavePreefKeySeri-*
        }

        SetScoreUnlockStageDanLainLain(PlayerPrefs.GetString(GameUtility.LastStageLoad));

        var TotalProgressScore = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-A") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-K") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-S") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-T") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-N") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-H") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-M") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-Y") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-R") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-W") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Konsonan Ganda") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Karakter Ganda");
        PlayerPrefs.SetInt(GameUtility.ProgessTotalScore, TotalProgressScore);

        var Trivia1Score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower A") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower K") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower S") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower T") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower N") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower H");
        PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower", Trivia1Score);

        var Trivia2Score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil M") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil Y") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil R") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil W") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil Konsonan Ganda") + PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil Karakter Ganda");
        PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Kuil", Trivia2Score);
    }

    private void SetScoreUnlockStageDanLainLain(string lastStageLoad)
    {
        //if (PlayerPrefs.GetString(lastStageLoad) == "Seri-A" && events.CurrentFinalScore > 1000) //minimal *1 untuk unlock level selanjutnya
        //{
        //    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-K", 1);
        //}
        switch (lastStageLoad)
        {
            case "Seri-A":
                if(events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-K", 1);
                }
                if(events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower A");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower A", score);
                }
                break;
            case "Seri-K":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-S", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower K");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower K", score);
                }
                break;
            case "Seri-S":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-T", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower S");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower S", score);
                }
                break;
            case "Seri-T":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-N", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower T");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower T", score);
                }
                break;
            case "Seri-N":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-H", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower N");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower N", score);
                }
                break;
            case "Seri-H":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-M", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Tokyo Tower H");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Tokyo Tower H", score);
                }
                break;
            case "Seri-M":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-Y", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil M");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Kuil M", score);
                }
                break;
            case "Seri-Y":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-R", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil Y");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Kuil Y", score);
                }
                break;
            case "Seri-R":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Seri-W", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil R");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Kuil R", score);
                }
                break;
            case "Seri-W":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Konsonan Ganda", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil W");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Kuil W", score);
                }
                break;
            case "Konsonan Ganda":
                if (events.CurrentFinalScore >= 1000)
                {
                    PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Karakter Ganda", 1);
                }
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil Konsonan Ganda");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Kuil Konsonan Ganda", score);
                }
                break;
            case "Karakter Ganda":
                if (events.CurrentFinalScore >= 1500)
                {
                    var score = PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Kuil Karakter Ganda");
                    score = 334;
                    PlayerPrefs.SetInt(GameUtility.SavePrefKey + "Kuil Karakter Ganda", score);
                }
                break;
        }
        if (PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-A") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-K") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-S") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-T") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-N") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-H") >= 1500) ;
        {
            PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Tokyo Tower", 1);
        }

        if (PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-M") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-Y") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-R") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Seri-W") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Konsonan Ganda") >= 1500 && PlayerPrefs.GetInt(GameUtility.SavePrefKey + "Karakter Ganda") >= 1500) ;
        {
            PlayerPrefs.SetInt(GameUtility.LevelUnlock + "Kuil", 1);
        }
    }

    private void UpdateScore(int add)
    {
        events.CurrentFinalScore += add;

        if (events.CurrentFinalScore < 0) { events.CurrentFinalScore = 0; }
        events.ScoreUpdated?.Invoke();
    }
}
