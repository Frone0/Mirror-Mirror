using UnityEngine;
using System.Collections;

public class Npc : MonoBehaviour
{
   public PoseAnimator poseAnimator;
   public characterState StateStore;

   Coroutine jumpRoutine;

   // jumpDelay: seconds to wait before the jump plays (hands and crouch change right away)
   public void SetPose(StateOfBodyParts state, float jumpDelay = 0f)
   {
        StateStore.CurrentState  = state;
        poseAnimator.SetPose(new StateOfBodyParts
        {
            LeftHandIsUp  = state.RightHandIsUp,
            RightHandIsUp = state.LeftHandIsUp,
            IsStanding    = state.IsStanding,
            Jumped        = false   // the jump is played below, so PoseAnimator doesn't jump right away
        });

        if (jumpRoutine != null) StopCoroutine(jumpRoutine);   // a new pose cancels a jump that hasn't happened yet
        jumpRoutine = null;

        if (state.Jumped) // every new jump pose jumps again, even if the last one was a jump too
        {
            jumpRoutine = StartCoroutine(JumpLater(jumpDelay));
        }
   }

   IEnumerator JumpLater(float delay)
   {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        poseAnimator.PlayJump();
        jumpRoutine = null;
   }
}
