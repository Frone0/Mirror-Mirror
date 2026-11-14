using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{

    public PoseAnimator poseAnimator;
    public characterState StateStore;

    void Start()
    {
    }

    // Update is called once per frame
  void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || poseAnimator == null) return;

        StateOfBodyParts pose = new StateOfBodyParts
        {
            LeftHandIsUp  = kb.aKey.isPressed || kb.leftArrowKey.isPressed,
            RightHandIsUp = kb.dKey.isPressed || kb.rightArrowKey.isPressed,
            IsStanding    = !(kb.sKey.isPressed || kb.downArrowKey.isPressed),
        };

        StateStore.CurrentState = pose;
        poseAnimator.SetPose(pose);
    }
}
