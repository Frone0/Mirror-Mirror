using UnityEngine;
using System;
using System.Collections.Generic;

public enum Language
{
    English, Russian
}

// All game texts in English and Russian.
// Use Loc.Get("key") in code, or put a LocalizedText component on a label.
public static class Loc
{
    const string PrefKey = "language";

    static Language current;
    static bool loaded;

    public static event Action OnLanguageChanged;

    public static Language Current
    {
        get { Load(); return current; }
        set
        {
            Load();
            if (current == value) return;
            current = value;
            PlayerPrefs.SetInt(PrefKey, (int)value);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
    }

    public static bool IsRussian => Current == Language.Russian;

    public static void Toggle()
    {
        Current = Current == Language.English ? Language.Russian : Language.English;
    }

    public static string Get(string key)
    {
        if (!Texts.TryGetValue(key, out string[] t))
        {
            Debug.LogWarning("Loc: no text for key '" + key + "'");
            return key;
        }
        string s = t[(int)Current];
        return string.IsNullOrEmpty(s) ? t[0] : s;   // no translation yet -> English
    }

    public static string Format(string key, params object[] args) => string.Format(Get(key), args);

    static void Load()   // saved choice, or the system language the first time
    {
        if (loaded) return;
        loaded = true;

        if (PlayerPrefs.HasKey(PrefKey))
        {
            current = (Language)PlayerPrefs.GetInt(PrefKey);
            return;
        }

        SystemLanguage sys = Application.systemLanguage;
        current = sys == SystemLanguage.Russian || sys == SystemLanguage.Ukrainian || sys == SystemLanguage.Belarusian
            ? Language.Russian
            : Language.English;
    }

    const string Y = "<color=#FFD040>";   // yellow key highlight
    const string E = "</color>";
    const string K = "<color=#B5452F>";   // red key highlight on the start panel

    //                               English                                   Russian
    static readonly Dictionary<string, string[]> Texts = new Dictionary<string, string[]>
    {
        // start menu
        { "menu.subtitle",   new[] { "Copy the reflection!",                     "Повторяй за отражением!" } },
        { "menu.controls",   new[] { K+"A"+E+" / "+K+"D"+E+"   raise left / right hand\n"+K+"S"+E+"   crouch\n"+K+"SPACE"+E+"   jump",
                                     K+"A"+E+" / "+K+"D"+E+"   поднять левую / правую руку\n"+K+"S"+E+"   присесть\n"+K+"ПРОБЕЛ"+E+"   прыжок" } },
        { "menu.play",       new[] { "PLAY",                                     "ИГРАТЬ" } },
        { "menu.training",   new[] { "Training Mode",                            "Тренировка" } },
        { "menu.language",   new[] { "ENG",                                      "РУС" } },

        // pause menu
        { "pause.title",     new[] { "PAUSE",                                    "ПАУЗА" } },
        { "pause.resume",    new[] { "CONTINUE",                                 "ПРОДОЛЖИТЬ" } },
        { "pause.restart",   new[] { "Restart",                                  "Заново" } },
        { "pause.menu",      new[] { "Menu",                                     "В меню" } },
        { "pause.hint",      new[] { K+"ESC"+E+"   pause / continue",            K+"ESC"+E+"   пауза / продолжить" } },

        // HUD
        { "hud.score",       new[] { "SCORE",                                    "СЧЁТ" } },
        { "mode.default",    new[] { "DEFAULT",                                  "ОБЫЧНЫЙ" } },
        { "mode.reverse",    new[] { "REVERSE",                                  "НАОБОРОТ" } },

        // mode change popup
        { "modeAlert.default", new[] { "DEFAULT MODE!!!",                        "ОБЫЧНЫЙ РЕЖИМ!!!" } },
        { "modeHint.default",  new[] { "Copy his pose!",                         "Повторяй его позу!" } },
        { "modeAlert.reverse", new[] { "REVERSE MODE!!!",                        "РЕЖИМ НАОБОРОТ!!!" } },
        { "modeHint.reverse",  new[] { "Do the opposite!",                       "Делай наоборот!" } },
        { "modeAlert.combo",   new[] { "COMBO MODE!!!",                          "РЕЖИМ КОМБО!!!" } },
        { "modeHint.combo",    new[] { "Copy or do opposite!",                   "Повторяй или делай наоборот!" } },
        { "mode.freeze",       new[] { "DO NOTHING",                             "НЕ ДВИГАЙСЯ" } },
        { "callout.freeze",    new[] { "DO NOTHING!",                            "НЕ ДВИГАЙСЯ!" } },

        // sound button
        { "sound.on",          new[] { "SOUND: ON",                              "ЗВУК: ВКЛ" } },
        { "sound.off",         new[] { "SOUND: OFF",                             "ЗВУК: ВЫКЛ" } },

        // combo callout (every combo level)
        { "combo.copy",        new[] { "COPY!",                                  "ПОВТОРЯЙ!" } },
        { "combo.opposite",    new[] { "OPPOSITE!",                              "НАОБОРОТ!" } },

        // training
        { "training.level",    new[] { "LEVEL {0}/{1}",                          "УРОВЕНЬ {0}/{1}" } },
        { "training.nice",     new[] { Y+"NICE!"+E,                              Y+"ОТЛИЧНО!"+E } },
        { "training.retry",    new[] { "<color=#FF5959>Not quite - try again!</color>", "<color=#FF5959>Не совсем - попробуй ещё!</color>" } },
        { "training.done",     new[] { Y+"TRAINING COMPLETE!"+E+"\nNow press PLAY", Y+"ТРЕНИРОВКА ПРОЙДЕНА!"+E+"\nТеперь нажми ИГРАТЬ" } },
    };
}
