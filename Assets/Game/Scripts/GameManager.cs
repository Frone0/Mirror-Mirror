using UnityEngine;

public class GameManager : MonoBehaviour
{
     public  PlayerControl Player;
     public  Npc NPC;

     int CorrectAnswersCount;

   public bool CheckMatch()
{
    StateOfBodyParts a = Player.StateStore.CurrentState;
    StateOfBodyParts b = NPC.StateStore.CurrentState;

    bool same = a.RightHandIsUp == b.RightHandIsUp
             && a.LeftHandIsUp  == b.LeftHandIsUp
             && a.IsStanding    == b.IsStanding;

    return same;
}

 public void ChangheScore(int count)
    {
        Score += count;
    }


}
