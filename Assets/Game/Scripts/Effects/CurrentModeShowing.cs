using UnityEngine;
using System.Collections;
using TMPro;

public class CurrentModeShowing : MonoBehaviour
{
  public TMP_Text Text;
  public Ui_Effects_Control UIControler;

  public float BumpSize = 1.3f;
  public float BumpTime = 0.25f;

  bool lastModeisComboAndDef;
  Vector3 startScale;
  Coroutine bumpRoutine;

   void Awake()
   {
        startScale = Text.transform.localScale;
   }

   void OnEnable()  {UIControler.GameFlow.OnLevelStarted += SetText;}
   void OnDisable() {UIControler.GameFlow.OnLevelStarted -= SetText;}

   public void SetText(float time)
   {
        switch (UIControler.GameManager.CurrentGameMode)
        {
            case GameMode.Default:
                Text.text = "DEFAULT";
                break;
            case GameMode.Reverse:
                Text.text = "REVERSE";
                break;
            case GameMode.Combo:
                if(UIControler.GameManager.IsComboAndDef) Text.text = "DEFAULT"; else Text.text = "REVERSE";
                CheckModeChange();
                break;
        }
   }

   void CheckModeChange()
   {
        if(UIControler.GameManager.IsComboAndDef != lastModeisComboAndDef)
        {
            lastModeisComboAndDef = UIControler.GameManager.IsComboAndDef;
            Bump();
        }
   }

   void Bump()
   {
        if (bumpRoutine != null) StopCoroutine(bumpRoutine);
        bumpRoutine = StartCoroutine(BumpEffect());
   }

   IEnumerator BumpEffect() 
   {
        float elapsedTime = 0f;

        while (elapsedTime < BumpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / BumpTime);
            float size = Mathf.Lerp(BumpSize, 1f, t);

            Text.transform.localScale = startScale * size;
            yield return null;
        }

        Text.transform.localScale = startScale;
        bumpRoutine = null;
   }
}
