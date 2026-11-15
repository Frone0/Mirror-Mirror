using UnityEngine;
using System.Collections;
using TMPro;
public class ChangedModeEffect : MonoBehaviour
{
    public GameObject EffectObject;
    public TMP_Text EffectText;
    public TMP_Text HintText;   
    public Ui_Effects_Control UIControler;

    public float AppearTime = 0.4f;     // grow from 0 to full size and fade in
    public float StayTime = 1f;         // how long the text stays on screen
    public float DisappearTime = 0.3f;  // fade out
    public float Overshoot = 1.35f;     // how much bigger than normal it gets while punching in
    public float PulseSize = 0.06f;     // heartbeat while it stays

    Coroutine effectRoutine;
    Vector3 fullScale;  

    void Awake()
    {
        fullScale = EffectObject.transform.localScale;
    }

    void Start()
    {
        EffectObject.SetActive(false);
    }

   void OnEnable()  {UIControler.GameManager.GetGameModeChangedEvent += SetText;}
   void OnDisable() {UIControler.GameManager.GetGameModeChangedEvent -= SetText;}

    void SetText(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Default:
                EffectText.text = Loc.Get("modeAlert.default");
                HintText.text = Loc.Get("modeHint.default");
                break;
            case GameMode.Reverse:
                EffectText.text = Loc.Get("modeAlert.reverse");
                HintText.text = Loc.Get("modeHint.reverse");
                break;
            case GameMode.Combo:
                EffectText.text = Loc.Get("modeAlert.combo");
                HintText.text = Loc.Get("modeHint.combo");
                break;
        }

        EffectText.color = ModeColors.Of(mode);   // blue copy / orange opposite / purple combo
        Animation();
    }

    void Animation()
    {
       
        if (effectRoutine != null) StopCoroutine(effectRoutine);

        EffectObject.SetActive(true);
        effectRoutine = StartCoroutine(FadeInEffect());
    }

    IEnumerator FadeInEffect() //Changing the size of the text and fading it in, then fading it out after a delay
    {
        CanvasGroup canvasGroup = EffectObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = EffectObject.AddComponent<CanvasGroup>();
        }

        Transform target = EffectObject.transform;
        float elapsedTime = 0f;

        while (elapsedTime < AppearTime)   // punches in: grows past full size, then settles
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / AppearTime);
            float size = t < 0.6f
                ? Mathf.Lerp(0.2f, Overshoot, 1f - (1f - t / 0.6f) * (1f - t / 0.6f))
                : Mathf.Lerp(Overshoot, 1f, (t - 0.6f) / 0.4f);

            target.localScale = fullScale * size;
            canvasGroup.alpha = Mathf.Clamp01(t * 2f);
            yield return null;
        }

        target.localScale = fullScale;
        canvasGroup.alpha = 1f;

        elapsedTime = 0f;
        while (elapsedTime < StayTime)   // gentle heartbeat while it's on screen
        {
            elapsedTime += Time.deltaTime;
            target.localScale = fullScale * (1f + Mathf.Abs(Mathf.Sin(elapsedTime * 6f)) * PulseSize);
            yield return null;
        }
        target.localScale = fullScale;

        yield return FadeOutEffect(canvasGroup);
    }

    IEnumerator FadeOutEffect(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;

        while (elapsedTime < DisappearTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / DisappearTime); 
            yield return null;
        }

        EffectObject.SetActive(false);
        canvasGroup.alpha = 1f; 
        effectRoutine = null;
    }
}
