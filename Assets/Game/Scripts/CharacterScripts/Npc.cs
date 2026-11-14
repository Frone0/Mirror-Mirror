using UnityEngine;

public class Npc : MonoBehaviour
{
   public PoseAnimator poseAnimator;
   public characterState StateStore;

   public void SetPose(StateOfBodyParts state)
   {
        StateStore.CurrentState  = state;
        poseAnimator.SetPose(new StateOfBodyParts
        {
            LeftHandIsUp  = state.RightHandIsUp,
            RightHandIsUp = state.LeftHandIsUp,
            IsStanding    = state.IsStanding,
            Jumped        = state.Jumped
        });

        if (state.Jumped) // every new jump pose jumps again, even if the last one was a jump too
        {
            poseAnimator.PlayJump();
        }
   }
}
