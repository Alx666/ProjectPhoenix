using UnityEngine;
using System.Collections;
using System;

public class DeathMatchWinCondition : IVictoryCondition
{
    public int MaxScore { get; private set; }

    public DeathMatchWinCondition(int iMaxScore)
    {
        MaxScore = iMaxScore;
    }

    public bool Check()
    {
        if (GameManager.Instance.GetHighestScore() >= MaxScore)
            return true;
        else
            return false;
    }
}
