using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Round timer at the top of the screen.
// The ring is full when a new pose appears and empties as time runs out.
public class LevelTimerUI : MonoBehaviour
{
    [Header("Links")]
    public GameFlow gameFlow;
    public Image fill;            // Image Type = Filled, Radial 360, origin Top
    public TMP_Text secondsText;
    public CanvasGroup group;     // used to hide the timer between games

    [Header("Look")]
    [Tooltip("Left = time is almost out, right = full time")]
    public Gradient colorOverTime;
    [Range(0f, 1f)] public float hurryBelow = 0.3f;   // start pulsing when less than 30% is left
    public float pulseSize = 0.08f;
    public float pulseSpeed = 10f;

    float duration;
    float endTime;
    bool running;

    void OnEnable()
    {
        gameFlow.OnLevelStarted += StartCountdown;
        gameFlow.OnGameStopped += StopCountdown;
    }

    void OnDisable()
    {
        gameFlow.OnLevelStarted -= StartCountdown;
        gameFlow.OnGameStopped -= StopCountdown;
    }

    void Awake()
    {
        group.alpha = 0f;   // nothing to show until the first pose
    }

    void StartCountdown(float seconds)
    {
        duration = seconds;
        endTime = Time.time + seconds;
        running = true;
        group.alpha = 1f;
    }

    void StopCountdown()
    {
        running = false;
        group.alpha = 0f;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        if (!running) return;

        float left = Mathf.Max(0f, endTime - Time.time);
        float t = duration > 0f ? left / duration : 0f;   // 1 = full, 0 = empty

        fill.fillAmount = t;
        fill.color = colorOverTime.Evaluate(t);
        secondsText.text = Mathf.CeilToInt(left).ToString();

        // little heartbeat when time is almost up
        float scale = 1f;
        if (t < hurryBelow && left > 0f)
            scale += Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseSize;
        transform.localScale = Vector3.one * scale;
    }

    // default colours when the component is added: blue -> gold -> red, like the web version
    void Reset()
    {
        colorOverTime = new Gradient();
        colorOverTime.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.94f, 0.35f, 0.31f), 0f),    // red
                new GradientColorKey(new Color(0.89f, 0.74f, 0.38f), 0.45f), // gold
                new GradientColorKey(new Color(0.56f, 0.88f, 1f), 1f),       // blue
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
    }
}
