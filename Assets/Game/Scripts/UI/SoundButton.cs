using UnityEngine;
using System;
using TMPro;

// Turns all game sound on/off. The choice is saved, so it stays off next time the game starts.
public static class SoundSettings
{
    const string PrefKey = "sound_muted";

    public static event Action OnChanged;

    public static bool Muted
    {
        get => PlayerPrefs.GetInt(PrefKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
            OnChanged?.Invoke();
        }
    }

    public static void Toggle() => Muted = !Muted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Apply() => AudioListener.volume = Muted ? 0f : 1f;
}

// Put on a button. Shows a speaker icon (on / off) - or, without an icon, the label SOUND: ON / SOUND: OFF.
// Icons: Google Material Icons (Apache License 2.0).
public class SoundButton : MonoBehaviour
{
    public TMP_Text Label;
    public UnityEngine.UI.Image Icon;
    public Sprite SoundOnSprite;
    public Sprite SoundOffSprite;

    void OnEnable()
    {
        SoundSettings.OnChanged += Refresh;
        Loc.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        SoundSettings.OnChanged -= Refresh;
        Loc.OnLanguageChanged -= Refresh;
    }

    public void Toggle() => SoundSettings.Toggle();

    void Refresh()
    {
        bool muted = SoundSettings.Muted;
        if (Icon != null)
        {
            Icon.sprite = muted ? SoundOffSprite : SoundOnSprite;
            if (Label != null) Label.gameObject.SetActive(false);   // the icon says it all
            return;
        }
        if (Label != null) Label.text = Loc.Get(muted ? "sound.off" : "sound.on");
    }
}
