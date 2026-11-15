using UnityEngine;

public class RandomLevelsGenerator : MonoBehaviour
{
   public StateOfBodyParts GetRandomLevel()
   {
   return new StateOfBodyParts{RightHandIsUp = GetRandomValue(),
   LeftHandIsUp = GetRandomValue(),
   IsStanding = GetRandomValue(),
   Jumped = GetRandomValue()
   };
   }

   bool GetRandomValue()
   {
    return Random.Range(0,2) == 0 ? true : false;
   }
}
