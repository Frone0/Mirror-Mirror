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
                 return CompareReverse(a, b);
             case GameMode.Combo:
                 return IsComboAndDef == true ? ComparePoses(a, b) : CompareReverse(a, b);
             default:
                 return false;
         }
     }

     bool CompareReverse(StateOfBodyParts a, StateOfBodyParts b) // a is player, b is npc
     {
         bool handsOpposite = a.RightHandIsUp != b.RightHandIsUp
                           && a.LeftHandIsUp  != b.LeftHandIsUp;

         if (b.Jumped) // he jumps - you dont, crouch doesnt matter
         {
             return handsOpposite && !a.Jumped;
         }

         if (b.IsStanding) // he stands - you crawl or jump, both are right
         {
             return handsOpposite && (!a.IsStanding || a.Jumped);
         }

         // he crawls - you stand and dont jump
         return handsOpposite && a.IsStanding && !a.Jumped;
     }

     bool ComparePoses(StateOfBodyParts a, StateOfBodyParts b)
     {
         bool handsSame = a.RightHandIsUp == b.RightHandIsUp
                       && a.LeftHandIsUp  == b.LeftHandIsUp;

         if (b.Jumped) // on jump poses only hands and jump count, crouch doesnt matter
         {
             return handsSame && a.Jumped;
         }

         return handsSame
             && a.IsStanding == b.IsStanding
             && !a.Jumped;
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
