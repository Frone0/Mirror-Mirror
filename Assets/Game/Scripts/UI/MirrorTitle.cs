using UnityEngine;
using TMPro;

// Game title: line 1 "MIRROR" in gold, line 2 "RORRIM" drawn as its real reflection
// (every letter flipped, glassy blue, fading out), plus a shine that sweeps across now and then.
[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class MirrorTitle : MonoBehaviour
{
    [Header("Name")]
    public Color TopColor = new Color(1f, 0.97f, 0.86f);
    public Color BottomColor = new Color(1f, 0.74f, 0.30f);

    [Header("Reflection (2nd line)")]
    public bool FlipLetters = true;
    public Color ReflectionTop = new Color(0.84f, 0.94f, 1f, 0.95f);
    public Color ReflectionBottom = new Color(0.55f, 0.76f, 0.96f, 0.30f);
    public float RippleAmount = 1.5f;   // letters of the reflection bob a tiny bit, like glass
    public float RippleSpeed = 2.5f;

    [Header("Shine")]
    public Color ShineColor = Color.white;
    public float ShineEvery = 3.5f;   // seconds between shines
    public float ShineTime = 0.8f;    // how long one sweep takes
    public float ShineWidth = 0.12f;  // part of the title the shine covers
    [Range(0f, 1f)] public float ShineStrength = 0.85f;

    TMP_Text text;

    void OnEnable()
    {
        text = GetComponent<TMP_Text>();
    }

    void OnDisable()
    {
        if (text != null) text.ForceMeshUpdate();   // back to the plain text
    }

    void LateUpdate()
    {
        if (text == null) return;

        text.ForceMeshUpdate();
        TMP_TextInfo info = text.textInfo;
        if (info.characterCount == 0) return;

        Bounds bounds = text.textBounds;
        float time = Time.realtimeSinceStartup;   // also runs in edit mode
        float shinePos = ShinePosition(time);

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo ch = info.characterInfo[i];
            if (!ch.isVisible) continue;

            int m = ch.materialReferenceIndex;
            int v = ch.vertexIndex;   // 0 bottom-left, 1 top-left, 2 top-right, 3 bottom-right
            Vector3[] verts = info.meshInfo[m].vertices;
            Color32[] colors = info.meshInfo[m].colors32;
            bool reflection = ch.lineNumber >= 1;

            Color top = reflection ? ReflectionTop : TopColor;
            Color bottom = reflection ? ReflectionBottom : BottomColor;

            if (reflection)
            {
                if (FlipLetters)   // mirror the glyph by swapping its left and right texture coords
                {
                    var uvs = info.meshInfo[m].uvs0;
                    (uvs[v + 0], uvs[v + 3]) = (uvs[v + 3], uvs[v + 0]);
                    (uvs[v + 1], uvs[v + 2]) = (uvs[v + 2], uvs[v + 1]);
                }

                if (RippleAmount > 0f && Application.isPlaying)
                {
                    Vector3 offset = Vector3.up * Mathf.Sin(time * RippleSpeed + i * 0.8f) * RippleAmount;
                    for (int k = 0; k < 4; k++) verts[v + k] += offset;
                }
            }

            for (int k = 0; k < 4; k++)
            {
                Color c = (k == 1 || k == 2) ? top : bottom;
                c = AddShine(c, verts[v + k], bounds, shinePos);
                colors[v + k] = c;
            }
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    float ShinePosition(float time)   // 0..1 across the title while sweeping, far away otherwise
    {
        if (ShineEvery <= 0f || ShineTime <= 0f) return -10f;
        float t = time % ShineEvery;
        if (t > ShineTime) return -10f;
        return Mathf.Lerp(-0.3f, 1.3f, t / ShineTime);
    }

    Color AddShine(Color c, Vector3 vertex, Bounds bounds, float shinePos)
    {
        if (bounds.size.x <= 0f) return c;
        float x = (vertex.x - bounds.min.x) / bounds.size.x;
        float y = (vertex.y - bounds.min.y) / Mathf.Max(1f, bounds.size.y);
        x += y * 0.25f;   // slanted band
        float glow = Mathf.Clamp01(1f - Mathf.Abs(x - shinePos) / ShineWidth) * ShineStrength;
        Color shine = ShineColor;
        shine.a = c.a;
        return Color.Lerp(c, shine, glow);
    }
}
