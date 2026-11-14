using UnityEngine;

public class Npc : MonoBehaviour
{
   public PoseAnimator poseAnimator;
   public characterState StateStore;

   public void SetPose(StateOfBodyParts state)
   {
        StateStore.CurrentState  = state;
        poseAnimator.SetPose(state);
   }
}
