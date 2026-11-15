using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class AnswerColorEffect : MonoBehaviour
{

    public GameObject EffectObject;
    public Color WrongColor;
    public Color RightColor;
    public Ui_Effects_Control UIControler;

    void OnEnable()  { UIControler.GameManager.GetDamageEvent += SetWrongColor;   UIControler.GameManager.GetScoreEvent += SetRightColor;}   // subscribeUIControler
    void OnDisable() { UIControler.GameManager.GetDamageEvent -= SetWrongColor;   UIControler.GameManager.GetScoreEvent -= SetRightColor;}   // always unsubscribe
    void Start()
    {
        EffectObject.SetActive(false);
    }

     void SetRightColor()
    {
        EffectObject.GetComponent<Image>().color = RightColor;
        ActivateEffect();
    }
     void SetWrongColor()
    {
         EffectObject.GetComponent<Image>().color = WrongColor;
        ActivateEffect();
    }

    void ActivateEffect()       
    {
    EffectObject.SetActive(true);
    StartCoroutine(FadeOutRedEffect());
    }

    IEnumerator FadeOutRedEffect()
    {
        float duration = 0.4f; // Duration of the fade-out effect in seconds
        float elapsedTime = 0f;

        CanvasGroup canvasGroup = EffectObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = EffectObject.AddComponent<CanvasGroup>();
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null;
        }

        EffectObject.SetActive(false);
        canvasGroup.alpha = 1f; // Reset alpha for next time
    }
}

