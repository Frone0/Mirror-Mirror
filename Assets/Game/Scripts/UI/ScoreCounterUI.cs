using UnityEngine;
using System.Collections;
using TMPro;

public class ScoreCounterUI : MonoBehaviour
{
    public TMP_Text ScoreText;
    public TMP_Text StreakText;
    public TMP_Text PopupPrefab;     // the little "+1" / "-1"
    public Transform PopupPlace;
    public Ui_Effects_Control UIControler;

    public Color NormalColor = new Color(0.95f, 0.93f, 0.9f);
    public Color RightColor = new Color(1f, 0.82f, 0.25f);
    public Color WrongColor = new Color(1f, 0.35f, 0.35f);

    public float CountTime = 0.3f;
    public float BumpSize = 1.3f;
    public float BumpTime = 0.3f;
    public float ShakeStrength = 6f;
    public float PopupTime = 0.7f;
    public float PopupHeight = 40f;
    public int StreakToShow = 3;

    int shownScore;
    int streak;
    Vector3 startScale;
    Vector3 startPosition;
    Coroutine countRoutine;
    Coroutine bumpRoutine;

    void Awake()
    {
        startScale = ScoreText.transform.localScale;
        startPosition = ScoreText.transform.localPosition;
    }

    void Start()
    {
        shownScore = UIControler.GameManager.Score;
        ScoreText.text = shownScore.ToString();
        ScoreText.color = BaseColor();
        StreakText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        UIControler.GameManager.GetScoreEvent += GotRight;
        UIControler.GameManager.GetDamageEvent += GotWrong;
    }
    void OnDisable()
    {
        UIControler.GameManager.GetScoreEvent -= GotRight;
        UIControler.GameManager.GetDamageEvent -= GotWrong;
    }

    void GotRight()
    {
        streak++;
        ShowStreak();

        float bump = BumpSize;
        if (streak >= StreakToShow) bump += 0.2f;   // bigger bump on a streak

        Changed(RightColor, bump, false);
        SpawnPopup("+1", RightColor);
    }

    void GotWrong()
    {
        streak = 0;
        ShowStreak();

        Changed(WrongColor, BumpSize, true);
        SpawnPopup("-1", WrongColor);
    }

    void Changed(Color color, float bump, bool shake)
    {
        if (countRoutine != null) StopCoroutine(countRoutine);
        countRoutine = StartCoroutine(CountEffect());

        if (bumpRoutine != null) StopCoroutine(bumpRoutine);
        bumpRoutine = StartCoroutine(BumpEffect(color, bump, shake));
    }

    Color BaseColor() // red when score is under zero
    {
        if (UIControler.GameManager.Score < 0) return WrongColor;
        return NormalColor;
    }

    IEnumerator CountEffect() // number rolls to the new score
    {
        int from = shownScore;
        int to = UIControler.GameManager.Score;
        float elapsedTime = 0f;

        while (elapsedTime < CountTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / CountTime);

            shownScore = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            ScoreText.text = shownScore.ToString();
            yield return null;
        }

        shownScore = to;
        ScoreText.text = shownScore.ToString();
        countRoutine = null;
    }

    IEnumerator BumpEffect(Color color, float bump, bool shake) // grows, flashes color and goes back
    {
        Transform target = ScoreText.transform;
        float elapsedTime = 0f;

        while (elapsedTime < BumpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / BumpTime);

            target.localScale = startScale * Mathf.Lerp(bump, 1f, t);
            ScoreText.color = Color.Lerp(color, BaseColor(), t);

            if (shake)
            {
                target.localPosition = startPosition + (Vector3)Random.insideUnitCircle * ShakeStrength * (1f - t);
            }
            yield return null;
        }

        target.localScale = startScale;
        target.localPosition = startPosition;
        ScoreText.color = BaseColor();
        bumpRoutine = null;
    }

    void ShowStreak()
    {
        if (streak >= StreakToShow)
        {
            StreakText.text = "x" + streak + "!";
            StreakText.gameObject.SetActive(true);
        }
        else
        {
            StreakText.gameObject.SetActive(false);
        }
    }

    void SpawnPopup(string text, Color color)
    {
        TMP_Text popup = Instantiate(PopupPrefab, PopupPlace);
        popup.text = text;
        popup.color = color;
        StartCoroutine(PopupEffect(popup));
    }

    IEnumerator PopupEffect(TMP_Text popup) // flies up and fades out
    {
        Transform target = popup.transform;
        Vector3 start = target.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < PopupTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / PopupTime);

            target.localPosition = start + Vector3.up * PopupHeight * t;
            popup.alpha = 1f - t * t;
            yield return null;
        }

        Destroy(popup.gameObject);
    }
}
