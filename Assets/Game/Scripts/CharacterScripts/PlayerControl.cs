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
        if (Time.timeScale == 0f) return;   // paused: keys don't change the pose

        bool aKey = kb.aKey.isPressed || kb.leftArrowKey.isPressed;
        bool dKey = kb.dKey.isPressed || kb.rightArrowKey.isPressed;
        bool swap = MirrorSettings.SwapKeys;   // mirror controls: A raises the right hand, D the left

        StateOfBodyParts pose = new StateOfBodyParts
        {
            LeftHandIsUp  = swap ? dKey : aKey,
            RightHandIsUp = swap ? aKey : dKey,
            IsStanding    = !(kb.sKey.isPressed || kb.downArrowKey.isPressed),
        };

        if(kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame )
        {
            jumpedThisLevel = true;
        }
        pose.Jumped = jumpedThisLevel;

        StateStore.CurrentState = pose;
        poseAnimator.SetPose(pose);
    }
}
