using UnityEngine;
using System;
public enum GameMode
{
    Default,Reverse,Combo
}
public class GameManager : MonoBehaviour
{
     public  PlayerControl Player;
     public  Npc NPC;
     public int Score;
     public GameMode CurrentGameMode;
     public int DifficultyLevel;
     public bool IsComboAndDef;

     public event Action GetDamageEvent; 
      public event Action GetScoreEvent; 
      public event Action<GameMode> GetGameModeChangedEvent;
     public bool CheckMatch()
     {
         StateOfBodyParts a = Player.StateStore.CurrentState;
         StateOfBodyParts b = NPC.StateStore.CurrentState;
     

        switch (CurrentGameMode)
         {
             case GameMode.Default:
                 return ComparePoses(a, b);
             case GameMode.Reverse:
                 return ComparePoses(a, ReversePose(b));
             case GameMode.Combo:
                 return IsComboAndDef == true ? ComparePoses(a, b) : ComparePoses(a, ReversePose(b));
             default:
                 return false;
         }
     }

        StateOfBodyParts ReversePose(StateOfBodyParts pose)
        {
            return new StateOfBodyParts
            {
                RightHandIsUp = !pose.RightHandIsUp,
                LeftHandIsUp  = !pose.LeftHandIsUp,
                IsStanding    = !pose.IsStanding,
                Jumped        = pose.Jumped   // jump stays same in reverse
            };
        }

     bool ComparePoses(StateOfBodyParts a, StateOfBodyParts b)
     {
         return a.RightHandIsUp == b.RightHandIsUp
             && a.LeftHandIsUp  == b.LeftHandIsUp
             && a.IsStanding    == b.IsStanding
             && a.Jumped        == b.Jumped;
     }

     public void SetGameMode(GameMode mode)
     {
         CurrentGameMode = mode;
         GetGameModeChangedEvent?.Invoke(mode);
     }

     public void ResetScore()
     {
         Score = 0;
     }


    public void ChangheScore(int count)
    {
        Score += count;

        if (count < 0)
        {
            GetDamageEvent?.Invoke();
        }
        if (count > 0)
        {
            GetScoreEvent?.Invoke();
        }
    }


}
