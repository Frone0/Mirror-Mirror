using UnityEngine;
using System;

// Mirror controls: when on, A and D swap - A raises the RIGHT hand, D the LEFT hand (arrows swap too),
// like moving in front of a real mirror. Off by default. The choice is saved.
public static class MirrorSettings
{
    const string PrefKey = "swap_keys";

    public static event Action OnChanged;

    public static bool SwapKeys
    {
        get => PlayerPrefs.GetInt(PrefKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }

    public static void Toggle() => SwapKeys = !SwapKeys;

    // In texts the keys are written as <color=...>A</color> / <color=...>D</color>.
    // When the keys are swapped, swap those letters too so instructions stay correct.
    public static string FixKeys(string text)
    {
        if (!SwapKeys || string.IsNullOrEmpty(text)) return text;
        return text.Replace(">A<", ">\u0001<").Replace(">D<", ">A<").Replace(">\u0001<", ">D<");
    }
}
