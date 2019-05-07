using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[Serializable()]
public struct UIManagerParameters
{
    [Header("Answer Options")]
    [SerializeField] float margins;
    public float Margins { get { return margins; } }

    [Header("Resolution Screen Options")]
    [SerializeField] Color correctBGColor;
    public Color CorrectBGColor { get { return correctBGColor; } }
    [SerializeField] Color incorrectBGColor;
    public Color IncorrectBGColor { get { return incorrectBGColor; } }
    [SerializeField] Color finalBGColor;
    public Color FinalBGColor { get { return finalBGColor; } }

}
[Serializable()]
public struct UIElements
{
    [SerializeField] RectTransform answerContentArea;
    public RectTransform AnswerContentArea { get { return answerContentArea; } }

    [SerializeField] Text questionInfoTextObject;
    public Text QuestionInfoTextObject { get { return questionInfoTextObject; } }

    [SerializeField] Text scoreText;
    public Text ScoreText { get { return scoreText; } }

    [Space]

    [SerializeField] Animator resolutionScreenAnimator;
    public Animator ResolutionScreenAnimator { get { return resolutionScreenAnimator; } }

    [SerializeField] Image resolutionBG;
    public Image ResolutionBG { get { return resolutionBG; } }

    [SerializeField] Text resolutionInfoText;
    public Text ResolutionInfoText { get { return resolutionInfoText; } }

    [SerializeField] Text resolutionScoreText;
    public Text ResolutionScoreText { get { return resolutionScoreText; } }

    [Space]

    [SerializeField] Text highscoreText;
    public Text HighscoreText { get { return highscoreText; } }

    [SerializeField] CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGrop { get { return mainCanvasGroup; } }

    [SerializeField] RectTransform finishUIElements;
    public RectTransform FinishUIElements { get { return finishUIElements; } }


}

public class UIManager : MonoBehaviour
{
    public enum ResolutionScreenType { Correct,Incorrect,Finish }

    [Header("References")]
    [SerializeField] GameEvents events;

    [Header("UI Elemets (Prefabs)")]
    [SerializeField] AnswersData answerPrefab;

    [SerializeField] UIElements uIElements;

    [Space]
    [SerializeField] UIManagerParameters parameters;

    List<AnswersData> currentAnswer = new List<AnswersData>();

    private int resStateParaHash = 0;

    private IEnumerator IE_DisplayTimedResolution;

    private void OnEnable()
    {
        events.UpdateQuestionUI += UpdateQuestionUI;
        events.DisplayResolutionScreen += DisplayResolution;
        events.ScoreUpdated += UpdateScoreUI;
    }

    private void OnDisable()
    {
        events.UpdateQuestionUI -= UpdateQuestionUI;
        events.DisplayResolutionScreen -= DisplayResolution;
        events.ScoreUpdated -= UpdateScoreUI;
    }
    void UpdateQuestionUI(Question question)
    {
        uIElements.QuestionInfoTextObject.text = question.Info;
        //Create Answer
        CreateAnswers(question);
    }

    private void Start()
    {
        UpdateScoreUI();
        resStateParaHash = Animator.StringToHash("ScreenState");
    }

    void DisplayResolution(ResolutionScreenType type, int score)
    {
        UpdateResUI(type, score);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 2);
        uIElements.MainCanvasGrop.blocksRaycasts = false;

        if(type != ResolutionScreenType.Finish)
        {
            if (IE_DisplayTimedResolution != null)
            {
                StopCoroutine(IE_DisplayTimedResolution);
            }
            IE_DisplayTimedResolution = DisplayTimeResolution();
            StartCoroutine(IE_DisplayTimedResolution);
        }

    }

    IEnumerator DisplayTimeResolution()
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.MainCanvasGrop.blocksRaycasts = true;
    }

    void UpdateResUI(ResolutionScreenType type, int score)
    {
        var highscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey + GameUtility.LastStageLoad);

        switch (type)
        {
            case ResolutionScreenType.Correct:
                uIElements.ResolutionBG.color = parameters.CorrectBGColor;
                uIElements.ResolutionInfoText.text = "Correct";
                uIElements.ResolutionScoreText.text = "+" + score;
                break;
            case ResolutionScreenType.Incorrect:
                uIElements.ResolutionBG.color = parameters.IncorrectBGColor;
                uIElements.ResolutionInfoText.text = "Wrong";
                uIElements.ResolutionScoreText.text = "";
                break;
            case ResolutionScreenType.Finish:
                uIElements.ResolutionBG.color = parameters.FinalBGColor;
                uIElements.ResolutionInfoText.text = "FINAL SCORE";

                StartCoroutine(CalculateScore());
                uIElements.FinishUIElements.gameObject.SetActive(true);
                uIElements.HighscoreText.gameObject.SetActive(true);

                //Display Highscore
                uIElements.HighscoreText.text = ((highscore > events.StartupHighscore) ? "<color=yellow>new </color>" : string.Empty) + "Highscore: " + highscore;
                break;
        }
    }

    IEnumerator CalculateScore()
    {
        if(events.CurrentFinalScore == 0)
        {
            uIElements.ResolutionScoreText.text = 0.ToString();
            yield break;
        }

        var scoreValue = 0;
        var scoreMoreThanZero = events.CurrentFinalScore > 0; 
        while ((scoreMoreThanZero)? scoreValue < events.CurrentFinalScore : scoreValue > events.CurrentFinalScore)
        {
            scoreValue += scoreMoreThanZero ? 1 : -1;
            uIElements.ResolutionScoreText.text = scoreValue.ToString();

            yield return null;
        }
    }

    void CreateAnswers(Question question)
    {
        EraseAnswers();

        float offset = 0 - parameters.Margins;
        for (int i = 0; i < question.Answers.Length; i++)
        {
            AnswersData newAnswer = (AnswersData)Instantiate(answerPrefab, uIElements.AnswerContentArea);
            newAnswer.UpdateData(question.Answers[i].Info,i);

            newAnswer.Rect.anchoredPosition = new Vector2(0,offset);

            offset -= (newAnswer.Rect.sizeDelta.y + parameters.Margins);
            uIElements.AnswerContentArea.sizeDelta = new Vector2(uIElements.AnswerContentArea.sizeDelta.x, offset * -1);

            currentAnswer.Add(newAnswer);
        }
    }
    void EraseAnswers()
    {
        foreach (var answer in currentAnswer)
        {
            Destroy(answer.gameObject);
        }
        currentAnswer.Clear();
    }

    void UpdateScoreUI()
    {
        uIElements.ScoreText.text = "Score: " + events.CurrentFinalScore;
    }
}
