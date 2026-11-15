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

   [SerializeField] Transform DefPos;
   [SerializeField] Transform ComboPos;


   void Awake()
   {
        startScale = Text.transform.localScale;
   }

   void OnEnable()  {UIControler.GameFlow.OnLevelStarted += SetText;}
   void OnDisable() {UIControler.GameFlow.OnLevelStarted -= SetText;}

   void Update()
   {
        // only while playing: hidden in the start menu, pause menu (time stopped) and after game over
        bool show = UIControler.GameFlow.GameIsRunning && Time.timeScale > 0f;
        if (Text.enabled != show) Text.enabled = show;
   }

   public void SetText(float time)
   {
     Text.gameObject.transform.position = DefPos.position;
        switch (UIControler.GameManager.CurrentGameMode)
        {
            case GameMode.Default:
                Text.text = Loc.Get("mode.default");
                break;
            case GameMode.Reverse:
                Text.text = Loc.Get("mode.reverse");
                break;
            case GameMode.Combo:
                if(UIControler.GameManager.IsComboAndDef) Text.text = Loc.Get("mode.default"); else Text.text = Loc.Get("mode.reverse");
                Text.gameObject.transform.position = ComboPos.position;
                CheckModeChange();
                break;
        }

        if (UIControler.GameManager.IsFreezeLevel) Text.text = Loc.Get("mode.freeze");   // random "do nothing" level

        Text.color = ModeColors.Rule(UIControler.GameManager);   // blue copy / orange opposite
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
