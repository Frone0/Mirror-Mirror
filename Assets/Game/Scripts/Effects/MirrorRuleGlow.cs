using UnityEngine;

public class MirrorRuleGlow : MonoBehaviour
{
    public GameManager gm;
    public GameFlow gameFlow;
    public Renderer target;

    [Range(0f, 1f)] public float BaseAlpha = 0.35f;    // steady glow while playing
    [Range(0f, 1f)] public float FlareAlpha = 1f;      // flash when the rule changes
    public float FlareFadeTime = 0.9f;
    public float ColorChangeSpeed = 10f;
    [Range(0f, 1f)] public float BreathSize = 0.25f;   // slow breathing so it feels alive

    Material mat;
    Color currentColor = ModeColors.Copy;
    Color targetColor = ModeColors.Copy;
    float visible;          // 0 = off (menu), 1 = on (playing)
    float flare;            // 1 right after a change, fades to 0
    Color lastRule = ModeColors.Copy;
    bool hadLevel;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        mat = target.material;
        mat.SetTexture("_BaseMap", MakeEdgeTexture(256, 175));
        Apply();
    }

    void OnEnable()
    {
        gameFlow.OnLevelStarted += LevelStarted;
        gameFlow.OnGameStopped += GameStopped;
        gm.GetGameModeChangedEvent += ModeChanged;
    }

    void OnDisable()
    {
        gameFlow.OnLevelStarted -= LevelStarted;
        gameFlow.OnGameStopped -= GameStopped;
        gm.GetGameModeChangedEvent -= ModeChanged;
    }

    void LevelStarted(float seconds)
    {
        Color rule = ModeColors.Rule(gm);   // blue copy / orange opposite / silver do nothing
        targetColor = rule;
        if (!hadLevel || rule != lastRule) Flare(targetColor);   // the rule is different from last level
        lastRule = rule;
        hadLevel = true;
    }

    void ModeChanged(GameMode mode)
    {
        Flare(ModeColors.Of(mode));   // big flash in the new mode's color (purple for combo)
    }

    void GameStopped()
    {
        hadLevel = false;
    }

    void Flare(Color color)
    {
        currentColor = color;   // jump straight to the new color, then ease to the rule color
        flare = 1f;
    }

    void Update()
    {
        float goal = gameFlow.GameIsRunning ? 1f : 0f;
        visible = Mathf.MoveTowards(visible, goal, Time.unscaledDeltaTime * 2f);
        flare = Mathf.MoveTowards(flare, 0f, Time.deltaTime / FlareFadeTime);
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * ColorChangeSpeed * (1f - flare));
        Apply();
    }

    void Apply()
    {
        float breath = 1f - BreathSize * 0.5f * (1f + Mathf.Sin(Time.time * 2.2f));
        float a = Mathf.Lerp(BaseAlpha * breath, FlareAlpha, flare * flare) * visible;
        Color c = currentColor;
        c.a = a;
        mat.SetColor(BaseColor, c);
        target.enabled = a > 0.001f;
    }

    static Texture2D MakeEdgeTexture(int w, int h)   // glow only along the frame, clear in the middle
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x / (w - 1f) * 2f - 1f);
                float dy = Mathf.Abs(y / (h - 1f) * 2f - 1f);
                float edge = Mathf.Max(Mathf.Pow(dx, 6f), Mathf.Pow(dy, 5f));
                px[y * w + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(edge) * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }
}
