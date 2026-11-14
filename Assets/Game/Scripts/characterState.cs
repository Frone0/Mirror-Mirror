using UnityEngine;

[System.Serializable] // lets the struct show up in the Inspector
public struct StateOfBodyParts
{
    public bool RightHandIsUp;
    public bool LeftHandIsUp;
    public bool IsStanding;
} 

[System.Serializable] // without this Unity leaves StateStore null and the game throws NullReferenceException
public class characterState : MonoBehaviour
{
 public StateOfBodyParts CurrentState;
}


