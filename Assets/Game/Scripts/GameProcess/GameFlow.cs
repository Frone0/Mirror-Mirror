using UnityEngine;
using System.Collections;
using System;

public class GameFlow : MonoBehaviour
{
    public StateOfBodyParts[] levels;
    public Npc NPC;
    public GameManager gm;
    public float _TimePerLevel;
    public int CurrentLevel;  
    public bool GameIsRunning;


    public event Action<float> OnLevelStarted; 
    public event Action OnGameStopped;

    void Start()
    {
    }

    public void StartGame()
    {
        GameIsRunning = true;
        StartCoroutine(LevelsTimer());
    }



    public IEnumerator LevelsTimer()
    {
        CurrentLevel = 0;

        while (GameIsRunning)
        {
            NPC.SetPose(levels[CurrentLevel]);
            OnLevelStarted?.Invoke(_TimePerLevel);
            yield return new WaitForSeconds(_TimePerLevel);

            if (gm.CheckMatch())
            {
                gm.ChangheScore(1);
            }
            else
            {
                gm.ChangheScore(-1);
            }

            CurrentLevel++;


            if(CurrentLevel % 5 == 0)
            {
                PassedAnotherFiveLevels();
            }

            if(gm.CurrentGameMode == GameMode.Combo)
            {
                int randomValue = UnityEngine.Random.Range(0, 2);
                if(randomValue == 0) gm.IsComboAndDef = true; else gm.IsComboAndDef = false;
            }   

            if(CurrentLevel >= levels.Length)
            {
                CurrentLevel = 0;
            }
        }

        StopGame();
        Debug.Log("Game over. Score: " + gm.Score);
    }

    void PassedAnotherFiveLevels()
    {
       gm.SetGameMode((GameMode)(((int)gm.CurrentGameMode + 1) % 3));
       if(_TimePerLevel > 1)
       {
       _TimePerLevel *= 0.9f;    
       }
    }

    public void StopGame()
    {
        GameIsRunning = false;
        OnGameStopped?.Invoke();
    }
}
