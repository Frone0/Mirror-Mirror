using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PoseAnimator : MonoBehaviour
{
    [Tooltip("The pose to show. Tick the boxes in Play mode to test.")]
    public StateOfBodyParts pose = new StateOfBodyParts { IsStanding = true };

    [Tooltip("Swap left and right hands, for the reflection in the mirror.")]
    public bool mirror;

    static readonly int RightUp = Animator.StringToHash("RightUp");
    static readonly int LeftUp = Animator.StringToHash("LeftUp");
    static readonly int Crouch = Animator.StringToHash("Crouch");

    Animator animator;

    void Awake() => animator = GetComponent<Animator>();

    // Call this from your game code, e.g. npcPose.SetPose(level[i]);
    public void SetPose(StateOfBodyParts newPose) => pose = newPose;

    // True when this character shows the same pose as `other` (hands mirrored if needed).
    public bool Matches(StateOfBodyParts other) =>
        pose.RightHandIsUp == other.RightHandIsUp &&
        pose.LeftHandIsUp == other.LeftHandIsUp &&
        pose.IsStanding == other.IsStanding;

    void Update()
    {
        bool right = pose.RightHandIsUp, left = pose.LeftHandIsUp;
        if (mirror) (right, left) = (left, right);
        animator.SetBool(RightUp, right);
        animator.SetBool(LeftUp, left);
        animator.SetBool(Crouch, !pose.IsStanding);
    }
}
