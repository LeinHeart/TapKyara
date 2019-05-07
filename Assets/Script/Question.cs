﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnswerType { Multi, Single }


[Serializable()]
public class Answer
{
    public string Info = String.Empty;
    public bool IsCorrect = false;

    public Answer() { }
}

[Serializable()]
public class Question
{
    public String Info = null;
    public Answer[] Answers = null;
    public Boolean UseTimer = false;
    public Int32 Timer = 0;
    public AnswerType Type = AnswerType.Single;
    public Int32 AddScore = 0;

    public Question() { }

    public List<int> GetCorrectAnswers()
    {
        List<int> CorrectAnswer = new List<int>();
        for (int i = 0; i < Answers.Length; i++)
        {
            if (Answers[i].IsCorrect)
            {
                CorrectAnswer.Add(i);
            }
        }
        return CorrectAnswer;
    }
}
