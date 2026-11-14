using UnityEngine;
using System.Collections;

public class PanelsBeatifulCloseOpenSmooth : MonoBehaviour
{
  public Transform PanelObject;     // the panel that grows (background only fades)
  public CanvasGroup Group;

  public float OpenTime = 0.35f;
  public float CloseTime = 0.2f;
  public float Overshoot = 1.08f;   // gets a bit bigger than normal, then back

  Vector3 fullScale;
  Coroutine panelRoutine;

  void Awake()
  {
        fullScale = PanelObject.localScale;
  }

  public void Open()
    {
        gameObject.SetActive(true);

        if (panelRoutine != null) StopCoroutine(panelRoutine);
        panelRoutine = StartCoroutine(OpenEffect());
    }

  public void Close()
    {
        if (!gameObject.activeInHierarchy) return;

        if (panelRoutine != null) StopCoroutine(panelRoutine);
        panelRoutine = StartCoroutine(CloseEffect());
    }

  IEnumerator OpenEffect() // grows from small to a bit too big, then settles
    {
        Group.interactable = false;
        float elapsedTime = 0f;

        while (elapsedTime < OpenTime)
        {
            elapsedTime += Time.unscaledDeltaTime;   
            float t = Mathf.Clamp01(elapsedTime / OpenTime);

            float size;
            if (t < 0.7f)
            {
                float grow = t / 0.7f;
                size = Overshoot * (1f - (1f - grow) * (1f - grow));   
            }
            else
            {
                size = Mathf.Lerp(Overshoot, 1f, (t - 0.7f) / 0.3f);
            }

            PanelObject.localScale = fullScale * size;
            Group.alpha = Mathf.Clamp01(t * 2f);
            yield return null;
        }

        PanelObject.localScale = fullScale;
        Group.alpha = 1f;
        Group.interactable = true;
        panelRoutine = null;
    }

  IEnumerator CloseEffect() // gets smaller and fades out
    {
        Group.interactable = false;
        float elapsedTime = 0f;

        while (elapsedTime < CloseTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / CloseTime);

            PanelObject.localScale = fullScale * Mathf.Lerp(1f, 0.6f, t * t);
            Group.alpha = 1f - t;
            yield return null;
        }

        gameObject.SetActive(false);
        PanelObject.localScale = fullScale;
        Group.alpha = 1f;
        panelRoutine = null;
    }
}
