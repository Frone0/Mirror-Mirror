using UnityEngine;

[ExecuteAlways]
public class MirrorRig : MonoBehaviour
{
    [Tooltip("The glass. Its local Z axis is the mirror normal. Empty = this object.")]
    public Transform Glass;
    [Tooltip("The character in front of the mirror (PlayerModel).")]
    public Transform RealCharacter;
    [Tooltip("The character behind the glass that plays the reflection (NpcModel).")]
    public Transform Reflection;

    [Tooltip("Keep the reflection snapped while you move things in the editor.")]
    public bool SnapInEditor = true;
    [Tooltip("Snap once when the game starts (not every frame, so jumps and poses still play).")]
    public bool SnapOnStart = true;

    Transform G => Glass ? Glass : transform;

    void Start()
    {
        if (Application.isPlaying && SnapOnStart) Snap();
    }

    void Update()
    {
        if (!Application.isPlaying && SnapInEditor) Snap();
    }

    [ContextMenu("Snap reflection now")]
    public void Snap()
    {
        if (RealCharacter == null || Reflection == null) return;

        Vector3 n = G.forward.normalized;
        Vector3 real = RealCharacter.position;
        float distance = Vector3.Dot(real - G.position, n);   // signed distance to the glass
        Vector3 pos = real - 2f * distance * n;

        // A mirror flips handedness; LookRotation keeps a proper rotation that faces back.
        // (Left/right hands are already swapped in Npc.SetPose.)
        Quaternion rot = Quaternion.LookRotation(
            Vector3.Reflect(RealCharacter.forward, n),
            Vector3.Reflect(RealCharacter.up, n));

        Vector3 parentScale = Reflection.parent ? Reflection.parent.lossyScale : Vector3.one;
        Vector3 realScale = RealCharacter.lossyScale;
        Vector3 scale = new Vector3(realScale.x / parentScale.x, realScale.y / parentScale.y, realScale.z / parentScale.z);

        // Only write when something changed, so the scene isn't marked dirty every editor tick.
        if ((Reflection.position - pos).sqrMagnitude > 1e-8f) Reflection.position = pos;
        if (Quaternion.Angle(Reflection.rotation, rot) > 0.01f) Reflection.rotation = rot;
        if ((Reflection.localScale - scale).sqrMagnitude > 1e-8f) Reflection.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (RealCharacter == null || Reflection == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(RealCharacter.position, Reflection.position);
        Gizmos.matrix = Matrix4x4.TRS(G.position, G.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(G.lossyScale.x, G.lossyScale.y, 0f));
    }
}
