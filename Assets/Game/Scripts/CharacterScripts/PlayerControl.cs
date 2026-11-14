using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{

    public PoseAnimator poseAnimator;
    public characterState StateStore;
    public GameFlow gameFlow;

    bool jumpedThisLevel; // stays true until the next pose, so CheckMatch can see it


    void Start()
    {
    }

    void OnEnable()  {gameFlow.OnLevelStarted += ResetJump;}
    void OnDisable() {gameFlow.OnLevelStarted -= ResetJump;}

    void ResetJump(float seconds)
    {
        jumpedThisLevel = false;
    }

    // Update is called once per frame
   void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || poseAnimator == null) return;

        StateOfBodyParts pose = new StateOfBodyParts
        {
            LeftHandIsUp = kb.aKey.isPressed || kb.leftArrowKey.isPressed,
            RightHandIsUp = kb.dKey.isPressed || kb.rightArrowKey.isPressed,
            IsStanding    = !(kb.sKey.isPressed || kb.downArrowKey.isPressed),
        };

        if(kb.spaceKey.wasPressedThisFrame)
        {
            jumpedThisLevel = true;
        }
        pose.Jumped = jumpedThisLevel;

        StateStore.CurrentState = pose;
        poseAnimator.SetPose(pose);
    }
}
