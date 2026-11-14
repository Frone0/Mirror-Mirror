using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Builds the pose animations for the Floreswa characters:
//   RightArmUp / RightArmDown, LeftArmUp / LeftArmDown, Stand / Crouch,
// two arm masks, and the MirrorPose Animator Controller (one layer per body part,
// so both hands and the crouch can be combined freely).
//
// Poses are computed from the model's real skeleton (metarig), so they fit the model.
// Runs once automatically when the controller is missing; rebuild any time from
// the menu: Mirror Mirror > Build Pose Animations.
public static class PoseAnimationBuilder
{
    const string ModelPath = "Assets/Floreswa/Models/male01_1.fbx";
    const string OutDir = "Assets/Game/Animations/Poses";
    const string ControllerPath = OutDir + "/MirrorPose.controller";

    // Tweak these to taste, then rebuild from the menu.
    const float ArmOutward = 0.25f;   // how far the raised arm leans to the side
    const float CrouchDepth = 0.32f;  // hip drop, as a share of leg length
    const float CrouchLean = 18f;     // torso lean forward in degrees when crouching

    [InitializeOnLoadMethod]
    static void AutoBuild()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) == null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) != null)
                Build();
        };
    }

    [MenuItem("Mirror Mirror/Build Pose Animations")]
    public static void Build()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null) { Debug.LogError("Pose builder: model not found at " + ModelPath); return; }
        EnsureFolder(OutDir);

        var clips = new Dictionary<string, AnimationClip>
        {
            ["RightArmDown"] = MakeClip(model, "RightArmDown", Arm(".R"), null),
            ["RightArmUp"]   = MakeClip(model, "RightArmUp",   Arm(".R"), r => RaiseArm(r, ".R")),
            ["LeftArmDown"]  = MakeClip(model, "LeftArmDown",  Arm(".L"), null),
            ["LeftArmUp"]    = MakeClip(model, "LeftArmUp",    Arm(".L"), r => RaiseArm(r, ".L")),
            ["Stand"]        = MakeClip(model, "Stand",  BodyBones, null, true),
            ["Crouch"]       = MakeClip(model, "Crouch", BodyBones, Crouch, true),
        };

        var maskR = MakeMask(model, "RightArmMask", "shoulder.R");
        var maskL = MakeMask(model, "LeftArmMask", "shoulder.L");
        MakeController(clips, maskR, maskL);

        AssetDatabase.SaveAssets();
        Debug.Log("Pose builder: animations, masks and MirrorPose.controller are ready in " + OutDir +
                  ". Select your character and use Mirror Mirror > Add Poses To Selected.");
    }

    [MenuItem("Mirror Mirror/Add Poses To Selected")]
    static void AddToSelected()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) { Build(); controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath); }
        foreach (var go in Selection.gameObjects)
        {
            var animator = go.GetComponent<Animator>();
            if (animator == null) animator = Undo.AddComponent<Animator>(go);
            Undo.RecordObject(animator, "Set pose controller");
            animator.runtimeAnimatorController = controller;
            if (go.GetComponent<PoseAnimator>() == null) Undo.AddComponent<PoseAnimator>(go);
            EditorUtility.SetDirty(go);
        }
    }

    [MenuItem("Mirror Mirror/Add Poses To Selected", true)]
    static bool AddToSelectedValid() => Selection.gameObjects.Length > 0;

    // ---------- which bones each clip writes ----------
    static string[] Arm(string side) => new[] { "upper_arm" + side, "forearm" + side, "hand" + side };
    static readonly string[] BodyBones =
        { "spine", "spine.001", "spine.004", "thigh.L", "shin.L", "foot.L", "thigh.R", "shin.R", "foot.R" };

    // ---------- poses (applied to a temporary copy of the model) ----------
    static void RaiseArm(Transform root, string side)
    {
        var upper = Find(root, "upper_arm" + side);
        var fore = Find(root, "forearm" + side);
        var hand = Find(root, "hand" + side);
        Frame(root, out var right, out var forward);
        var outward = side == ".R" ? right : -right;
        var target = (Vector3.up + outward * ArmOutward + forward * 0.08f).normalized;
        upper.rotation = Quaternion.FromToRotation(fore.position - upper.position, target) * upper.rotation;
        fore.rotation = Quaternion.FromToRotation(hand.position - fore.position, target) * fore.rotation; // straight arm
    }

    static void Crouch(Transform root)
    {
        Frame(root, out var right, out var forward);
        var hips = Find(root, "spine");
        var legs = new[] { ".L", ".R" };
        var feet = new Dictionary<string, (Vector3 pos, Quaternion rot)>();
        float legLength = 0f;
        foreach (var s in legs)
        {
            var thigh = Find(root, "thigh" + s); var shin = Find(root, "shin" + s); var foot = Find(root, "foot" + s);
            feet[s] = (foot.position, foot.rotation);
            legLength = Vector3.Distance(thigh.position, shin.position) + Vector3.Distance(shin.position, foot.position);
        }

        hips.position -= Vector3.up * legLength * CrouchDepth;

        foreach (var s in legs)
        {
            var thigh = Find(root, "thigh" + s); var shin = Find(root, "shin" + s); var foot = Find(root, "foot" + s);
            float l1 = Vector3.Distance(thigh.position, shin.position), l2 = Vector3.Distance(shin.position, foot.position);
            Vector3 h = thigh.position, f = feet[s].pos;
            float d = Mathf.Min(Vector3.Distance(h, f), l1 + l2 - 0.0001f);
            var dir = (f - h).normalized;
            float a = (l1 * l1 - l2 * l2 + d * d) / (2f * d);
            float k = Mathf.Sqrt(Mathf.Max(0f, l1 * l1 - a * a));
            var bendDir = (forward - dir * Vector3.Dot(forward, dir)).normalized; // knees go forward
            var knee = h + dir * a + bendDir * k;
            thigh.rotation = Quaternion.FromToRotation(shin.position - thigh.position, knee - h) * thigh.rotation;
            shin.rotation = Quaternion.FromToRotation(foot.position - shin.position, f - shin.position) * shin.rotation;
            foot.rotation = feet[s].rot; // keep feet flat on the floor
        }

        var chest = Find(root, "spine.001");
        chest.rotation = Quaternion.AngleAxis(CrouchLean, right) * chest.rotation;
        var neck = Find(root, "spine.004");
        if (neck != null) neck.rotation = Quaternion.AngleAxis(-CrouchLean * 0.7f, right) * neck.rotation; // keep looking ahead
    }

    // Character's right (from .L to .R shoulder) and forward.
    static void Frame(Transform root, out Vector3 right, out Vector3 forward)
    {
        var r = Find(root, "upper_arm.R").position - Find(root, "upper_arm.L").position;
        r.y = 0f; right = r.normalized;
        forward = Vector3.Cross(right, Vector3.up).normalized;
    }

    // ---------- assets ----------
    static AnimationClip MakeClip(GameObject model, string name, string[] bones, System.Action<Transform> pose, bool hipPosition = false)
    {
        var inst = Object.Instantiate(model);
        inst.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            var root = inst.transform;
            root.position = Vector3.zero; root.rotation = Quaternion.identity; root.localScale = Vector3.one;
            pose?.Invoke(root);

            var clip = new AnimationClip { name = name, frameRate = 30 };
            foreach (var b in bones)
            {
                var t = Find(root, b);
                if (t == null) { Debug.LogWarning("Pose builder: bone not found: " + b); continue; }
                var path = AnimationUtility.CalculateTransformPath(t, root);
                var q = t.localRotation;
                Key(clip, path, "m_LocalRotation.x", q.x); Key(clip, path, "m_LocalRotation.y", q.y);
                Key(clip, path, "m_LocalRotation.z", q.z); Key(clip, path, "m_LocalRotation.w", q.w);
                if (hipPosition && b == "spine")
                {
                    var p = t.localPosition;
                    Key(clip, path, "m_LocalPosition.x", p.x); Key(clip, path, "m_LocalPosition.y", p.y); Key(clip, path, "m_LocalPosition.z", p.z);
                }
            }
            clip.EnsureQuaternionContinuity();

            var assetPath = OutDir + "/" + name + ".anim";
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (existing != null) { EditorUtility.CopySerialized(clip, existing); existing.name = name; return existing; }
            AssetDatabase.CreateAsset(clip, assetPath);
            return clip;
        }
        finally { Object.DestroyImmediate(inst); }
    }

    static void Key(AnimationClip clip, string path, string prop, float v)
    {
        var curve = new AnimationCurve(new Keyframe(0f, v), new Keyframe(0.5f, v)); // a held pose
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), prop), curve);
    }

    static AvatarMask MakeMask(GameObject model, string name, string fromBone)
    {
        var assetPath = OutDir + "/" + name + ".mask";
        AssetDatabase.DeleteAsset(assetPath);
        var mask = new AvatarMask();
        mask.AddTransformPath(model.transform, true);
        for (int i = 0; i < mask.transformCount; i++)
        {
            var path = mask.GetTransformPath(i);
            mask.SetTransformActive(i, path.EndsWith("/" + fromBone) || path.Contains("/" + fromBone + "/"));
        }
        AssetDatabase.CreateAsset(mask, assetPath);
        return mask;
    }

    static void MakeController(Dictionary<string, AnimationClip> clips, AvatarMask maskR, AvatarMask maskL)
    {
        // Rebuild in place when it already exists, so characters that use it keep the link.
        var c = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (c == null) c = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        else
        {
            foreach (var p in c.parameters) c.RemoveParameter(p);
            for (int i = c.layers.Length - 1; i >= 1; i--) c.RemoveLayer(i);
            var baseSm = c.layers[0].stateMachine;
            foreach (var s in baseSm.states) baseSm.RemoveState(s.state);
        }
        c.AddParameter("RightUp", AnimatorControllerParameterType.Bool);
        c.AddParameter("LeftUp", AnimatorControllerParameterType.Bool);
        c.AddParameter("Crouch", AnimatorControllerParameterType.Bool);

        // Base layer: stand / crouch
        var layers = c.layers;
        layers[0].name = "Body";
        c.layers = layers;
        TwoStates(c.layers[0].stateMachine, "Stand", clips["Stand"], "Crouch", clips["Crouch"], "Crouch");

        AddArmLayer(c, "RightArm", maskR, clips["RightArmDown"], clips["RightArmUp"], "RightUp");
        AddArmLayer(c, "LeftArm", maskL, clips["LeftArmDown"], clips["LeftArmUp"], "LeftUp");
        EditorUtility.SetDirty(c);
    }

    static void AddArmLayer(AnimatorController c, string name, AvatarMask mask, AnimationClip down, AnimationClip up, string param)
    {
        c.AddLayer(name);
        var layers = c.layers;
        int i = layers.Length - 1;
        layers[i].defaultWeight = 1f;
        layers[i].avatarMask = mask;
        c.layers = layers;
        TwoStates(c.layers[i].stateMachine, "Down", down, "Up", up, param);
    }

    static void TwoStates(AnimatorStateMachine sm, string offName, Motion off, string onName, Motion on, string param)
    {
        var a = sm.AddState(offName, new Vector3(260, 60));
        var b = sm.AddState(onName, new Vector3(260, 160));
        a.motion = off; b.motion = on;
        sm.defaultState = a;
        Link(a, b, AnimatorConditionMode.If, param);
        Link(b, a, AnimatorConditionMode.IfNot, param);
    }

    static void Link(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string param)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = 0.12f; // smooth, but quick enough for a rhythm game
        t.AddCondition(mode, 0f, param);
    }

    // ---------- helpers ----------
    static Transform Find(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform ch in t) { var r = Find(ch, name); if (r != null) return r; }
        return null;
    }

    static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        var cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
