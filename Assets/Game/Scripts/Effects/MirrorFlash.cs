using UnityEngine;
using System.Collections;

// Right / wrong flash on the mirror glass itself: the glass glows green or red (stronger at the edges)
// and a light in front of it tints the room for a moment.
// Put this on a transparent quad right in front of the mirror (behind the cracks).
public class MirrorFlash : MonoBehaviour
{
    public GameManager gm;
    public Renderer target;    // the glow quad
    public Light glowLight;    // optional, lights up the room in the flash color

    public Color RightColor = new Color(0.35f, 1f, 0.45f);
    public Color WrongColor = new Color(1f, 0.25f, 0.2f);

    public float RiseTime = 0.06f;
    public float FadeTime = 0.55f;
    [Range(0f, 1f)] public float MaxAlpha = 0.75f;
    public float LightIntensity = 4f;

    [Range(0f, 1f)] public float CenterStrength = 0.3f;   // glass middle is lighter than the edges

    Material mat;
    Coroutine flashRoutine;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        mat = target.material;   // own copy
        mat.SetTexture("_BaseMap", MakeGlowTexture(256, 175));
        SetStrength(Color.clear, 0f);
    }

    void OnEnable()
    {
        gm.GetScoreEvent += Right;
        gm.GetDamageEvent += Wrong;
    }

    void OnDisable()
    {
        gm.GetScoreEvent -= Right;
        gm.GetDamageEvent -= Wrong;
    }

    void Right() => Flash(RightColor);
    void Wrong() => Flash(WrongColor);

    void Flash(Color color)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashEffect(color));
    }

    IEnumerator FlashEffect(Color color)
    {
        float elapsedTime = 0f;
        while (elapsedTime < RiseTime)   // quick flash in
        {
            elapsedTime += Time.deltaTime;
            SetStrength(color, Mathf.Clamp01(elapsedTime / RiseTime));
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < FadeTime)   // slow fade out
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / FadeTime);
            SetStrength(color, (1f - t) * (1f - t));
            yield return null;
        }

        SetStrength(color, 0f);
        flashRoutine = null;
    }

    void SetStrength(Color color, float strength)
    {
        Color c = color;
        c.a = MaxAlpha * strength;
        mat.SetColor(BaseColor, c);
        target.enabled = strength > 0f;

        if (glowLight != null)
        {
            glowLight.color = color;
            glowLight.intensity = LightIntensity * strength;
            glowLight.enabled = strength > 0f;
        }
    }

    Texture2D MakeGlowTexture(int w, int h)   // white, alpha stronger near the frame
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x / (w - 1f) * 2f - 1f);   // 0 middle, 1 edge
                float dy = Mathf.Abs(y / (h - 1f) * 2f - 1f);
                float edge = Mathf.Max(Mathf.Pow(dx, 4f), Mathf.Pow(dy, 4f));
                float a = Mathf.Lerp(CenterStrength, 1f, edge);
                px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }
}
