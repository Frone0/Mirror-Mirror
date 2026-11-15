using UnityEngine;
using System;
using System.Collections;

// 3 tries. Every wrong pose cracks the mirror, the 3rd one shatters it and the game is over.
// Training mode doesn't take HP.
public class HealthSystem : MonoBehaviour
{
    public GameManager gm;
    public GameFlow gameFlow;
    public PanelsBeatifulCloseOpenSmooth StartPanel;   // opens again after game over

    public int MaxHP = 3;
    public int CurrentHP;
    public float GameOverTime = 3f;   // time to look at the shattered mirror before the menu opens

    public event Action<int> OnHPChanged;   // new HP value (MirrorCracks listens to this)
    public event Action OnDied;

    void Awake()
    {
        CurrentHP = MaxHP;
    }

    void OnEnable()
    {
        gm.GetDamageEvent += TakeDamage;
        gameFlow.OnGameStarted += ResetHP;
    }

    void OnDisable()
    {
        gm.GetDamageEvent -= TakeDamage;
        gameFlow.OnGameStarted -= ResetHP;
    }

    void ResetHP()
    {
        StopAllCoroutines();
        CurrentHP = MaxHP;
        OnHPChanged?.Invoke(CurrentHP);
    }

    void TakeDamage()
    {
        if (gm.StartMode == GameStartingMode.Training) return;   // training is for learning, no HP lost
        if (CurrentHP <= 0) return;

        CurrentHP--;
        OnHPChanged?.Invoke(CurrentHP);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        gameFlow.StopGame();
        OnDied?.Invoke();
        StartCoroutine(GameOver());
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(GameOverTime);
        if (StartPanel != null) StartPanel.Open();
    }
}
