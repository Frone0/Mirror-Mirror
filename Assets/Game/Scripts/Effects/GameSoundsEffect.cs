using UnityEngine;
using System.Collections;
public class GameSoundsEffect : MonoBehaviour
{
    public AudioSource Source;
    public AudioSource MusicSource;
    public Ui_Effects_Control UIControler;

    public AudioClip RightSound;
    public AudioClip WrongSound;
    public AudioClip ModeChangeSound;
    public AudioClip TickSound;
    public AudioClip Music;

    public float Volume = 0.8f;
    public float MusicVolume = 0.3f;
    public int TickSeconds = 2;   // ticks in the last seconds of every pose

    Coroutine tickRoutine;

    void Start()
    {
        MusicSource.clip = Music;
        MusicSource.loop = true;
        MusicSource.volume = MusicVolume;
        MusicSource.Play();
    }

    void OnEnable()
    {
        UIControler.GameManager.GetScoreEvent += PlayRight;
        UIControler.GameManager.GetDamageEvent += PlayWrong;
        UIControler.GameManager.GetGameModeChangedEvent += PlayModeChange;
        UIControler.GameFlow.OnLevelStarted += StartTicks;
    }
    void OnDisable()
    {
        UIControler.GameManager.GetScoreEvent -= PlayRight;
        UIControler.GameManager.GetDamageEvent -= PlayWrong;
        UIControler.GameManager.GetGameModeChangedEvent -= PlayModeChange;
        UIControler.GameFlow.OnLevelStarted -= StartTicks;
    }

    void PlayRight()
    {
        Source.PlayOneShot(RightSound, Volume);
    }

    void PlayWrong()
    {
        Source.PlayOneShot(WrongSound, Volume);
    }

    void PlayModeChange(GameMode mode)
    {
        Source.PlayOneShot(ModeChangeSound, Volume);
    }

    void StartTicks(float seconds)
    {
        if (tickRoutine != null) StopCoroutine(tickRoutine);
        tickRoutine = StartCoroutine(Ticks(seconds));
    }

    IEnumerator Ticks(float seconds)
    {
        float wait = seconds - TickSeconds;
        if (wait > 0f)
        {
            yield return new WaitForSeconds(wait);
        }

        for (int i = 0; i < TickSeconds; i++)
        {
            Source.PlayOneShot(TickSound, Volume);
            yield return new WaitForSeconds(1f);
        }

        tickRoutine = null;
    }
}
