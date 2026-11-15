using UnityEngine;
using System.Collections;
using System;

public class GameFlow : MonoBehaviour
{
    public StateOfBodyParts[] levels = new StateOfBodyParts[5];
    public Npc NPC;
    public GameManager gm;
    public float _TimePerLevel;
    public int CurrentLevel;  
    public bool GameIsRunning;

    public RandomLevelsGenerator RLG;
    public TrainingMode Training;

    [Range(0f, 1f)] public float FreezeChance = 0.15f;   // chance that a level is a "do nothing" level
    int levelsPlayed;


    public event Action<float> OnLevelStarted;
    public event Action OnGameStarted;   // HP, cracks and score reset on this
    public event Action OnGameStopped;

    float startTimePerLevel;   // levels get faster during a run, this brings the speed back on restart

    void Awake()
    {
        startTimePerLevel = _TimePerLevel;
    }

    public void StartGame()
    {
        if (GameIsRunning) return;   // don't start a second game on top of a running one
        GameIsRunning = true;
        gm.IsFreezeLevel = false;
        levelsPlayed = 0;

        if(gm.StartMode == GameStartingMode.Infinity)
        {
            _TimePerLevel = startTimePerLevel;
            gm.ResetScore();
            gm.CurrentGameMode = GameMode.Default;
        }

        OnGameStarted?.Invoke();

        if(gm.StartMode == GameStartingMode.Training)
        {
            Training.Run();
            return;
        }

        if(gm.StartMode == GameStartingMode.Infinity)
        {
        FillRandomLevels();   // fill before the timer starts, so the first pose is random too
        }

        StartCoroutine(LevelsTimer());
    }

    // Training Mode button calls this
    public void StartTraining()
    {
        gm.SetTraining();
        StartGame();
    }

    // events can only be called from inside this class, so TrainingMode uses this
    public void RaiseLevelStarted(float seconds)
    {
        OnLevelStarted?.Invoke(seconds);
    }

   public void FillRandomLevels()
   {
      for(int i = 0;i < levels.Length ; i ++)
      {
         levels[i] = RLG.GetRandomLevel(); 
      }
   }



    public IEnumerator LevelsTimer()
    {
        CurrentLevel = 0;

        while (GameIsRunning)
        {
            // random "do nothing" level: never the first level, never two in a row
            gm.IsFreezeLevel = levelsPlayed > 0 && !gm.IsFreezeLevel && UnityEngine.Random.value < FreezeChance;
            levelsPlayed++;

            float time = _TimePerLevel;
            if(gm.CurrentGameMode == GameMode.Reverse && !gm.IsFreezeLevel)
            {
                time++;
            }


            NPC.SetPose(levels[CurrentLevel], time * 0.5f);  
            OnLevelStarted?.Invoke(time);
            yield return new WaitForSeconds(time);

            if (gm.CheckMatch())
            {
                gm.ChangheScore(1);
            }
            else
            {
                gm.ChangheScore(-1);   // HealthSystem takes 1 HP here and can end the game
            }

            if (!GameIsRunning) yield break;   // last HP lost -> HealthSystem already stopped the game

            CurrentLevel++;


            if(levelsPlayed % 5 == 0)   // levelsPlayed never wraps (CurrentLevel goes back to 0 after the last slot)
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
       if(_TimePerLevel > 1.5)
       {
       _TimePerLevel *= 0.85f;    
       }

       FillRandomLevels();
    }

    
    public void AbortGame()
    {
        StopAllCoroutines();   // the level loop
        if (Training != null) Training.Abort();
        if (GameIsRunning) StopGame();
        gm.StartMode = GameStartingMode.Infinity;
        gm.CurrentGameMode = GameMode.Default;
        gm.IsFreezeLevel = false;
        NPC.SetPose(new StateOfBodyParts { IsStanding = true });
    }

    public void StopGame()
    {
        GameIsRunning = false;
        OnGameStopped?.Invoke();
    }
}
