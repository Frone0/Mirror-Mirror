using UnityEngine;
using System.Collections;
using TMPro;

// In combo mode the rule changes every level, so every level shouts the rule:
// big "COPY!" (blue) or "OPPOSITE!" (orange). A sound plays when the rule flipped from last level.
// Random "do nothing" levels (any mode) shout "DO NOTHING!" (silver).
public class ComboCallout : MonoBehaviour
{
    public GameManager gm;
    public GameFlow gameFlow;
    public TMP_Text Text;
    public AudioSource Source;
    public AudioClip FlipSound;
    [Range(0f, 1f)] public float FlipVolume = 0.6f;

    public float PunchTime = 0.18f;
    public float StayTime = 0.55f;
    public float FadeTime = 0.3f;
    public float StartSize = 2.2f;     // starts huge and slams down to normal size
    public float FlipShake = 10f;      // extra shake when the rule flipped

    Coroutine routine;
    Vector3 fullScale;
    Vector3 startPos;
    bool lastWasCopy;
    bool shownBefore;

    void Awake()
    {
        fullScale = Text.transform.localScale;
        startPos = Text.transform.localPosition;
        Text.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        gameFlow.OnLevelStarted += LevelStarted;
        gameFlow.OnGameStopped += Hide;
    }

    void OnDisable()
    {
        gameFlow.OnLevelStarted -= LevelStarted;
        gameFlow.OnGameStopped -= Hide;
    }

    void LevelStarted(float seconds)
    {
        if (gm.IsFreezeLevel)   // random "do nothing" level, in any mode: always shout it, with sound
        {
            Text.text = Loc.Get("callout.freeze");
            Text.color = ModeColors.Freeze;
            if (Source != null && FlipSound != null) Source.PlayOneShot(FlipSound, FlipVolume);
            shownBefore = false;   // the next combo level counts as a change again
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Show(true));
            return;
        }

        if (gm.CurrentGameMode != GameMode.Combo)
        {
            shownBefore = false;
            return;
        }

        bool copy = ModeColors.IsCopyRule(gm);
        bool flipped = !shownBefore || copy != lastWasCopy;
        lastWasCopy = copy;
        shownBefore = true;

        Text.text = Loc.Get(copy ? "combo.copy" : "combo.opposite");
        Text.color = copy ? ModeColors.Copy : ModeColors.Opposite;

        if (flipped && Source != null && FlipSound != null) Source.PlayOneShot(FlipSound, FlipVolume);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Show(flipped));
    }

    void Hide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        shownBefore = false;
        Text.gameObject.SetActive(false);
    }

    IEnumerator Show(bool flipped)
    {
        Transform t = Text.transform;
        Text.gameObject.SetActive(true);
        Text.alpha = 1f;

        float elapsedTime = 0f;
        while (elapsedTime < PunchTime)   // slams in from big to normal
        {
            elapsedTime += Time.deltaTime;
            float k = Mathf.Clamp01(elapsedTime / PunchTime);
            t.localScale = fullScale * Mathf.Lerp(StartSize, 1f, 1f - (1f - k) * (1f - k));
            Text.alpha = k;
            yield return null;
        }
        t.localScale = fullScale;

        elapsedTime = 0f;
        while (elapsedTime < StayTime)   // shakes for a moment if the rule changed
        {
            elapsedTime += Time.deltaTime;
            float shake = flipped ? FlipShake * (1f - elapsedTime / StayTime) : 0f;
            t.localPosition = startPos + (Vector3)(Random.insideUnitCircle * shake);
            yield return null;
        }
        t.localPosition = startPos;

        elapsedTime = 0f;
        while (elapsedTime < FadeTime)
        {
            elapsedTime += Time.deltaTime;
            Text.alpha = 1f - Mathf.Clamp01(elapsedTime / FadeTime);
            yield return null;
        }

        Text.gameObject.SetActive(false);
        routine = null;
    }
}
