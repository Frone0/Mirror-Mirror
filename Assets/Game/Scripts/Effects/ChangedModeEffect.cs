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
                EffectText.text = "DEFAULT MODE!!!";
                HintText.text = "Copy his pose!";
                break;
            case GameMode.Reverse:
                EffectText.text = "REVERSE MODE!!!";
                HintText.text = "Do the opposite!";
                break;
            case GameMode.Combo:
                EffectText.text = "COMBO MODE!!!";
                HintText.text = "Copy or do opposite!";
                break;
        }

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

        while (elapsedTime < AppearTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / AppearTime);
            float smooth = 1f - (1f - t) * (1f - t); 

            target.localScale = fullScale * smooth;     
            canvasGroup.alpha = t;                      
            yield return null;
        }

        target.localScale = fullScale;
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(StayTime); 

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
