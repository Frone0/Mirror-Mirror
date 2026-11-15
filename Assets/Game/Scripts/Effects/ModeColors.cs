using UnityEngine;

// One color per rule, used by every mode effect so the player learns them:
// blue = copy him, orange = do the opposite, purple = combo (changes every level), silver = do nothing.
public static class ModeColors
{
    public static readonly Color Copy = new Color(0.30f, 0.80f, 1f);
    public static readonly Color Opposite = new Color(1f, 0.55f, 0.12f);
    public static readonly Color Combo = new Color(0.82f, 0.42f, 1f);
    public static readonly Color Freeze = new Color(0.88f, 0.92f, 0.98f);

    // the rule for the current level (in combo it's decided per level)
    public static bool IsCopyRule(GameManager gm)
    {
        return gm.CurrentGameMode == GameMode.Default
            || (gm.CurrentGameMode == GameMode.Combo && gm.IsComboAndDef);
    }

    public static Color Rule(GameManager gm)
    {
        if (gm.IsFreezeLevel) return Freeze;   // random "do nothing" level
        return IsCopyRule(gm) ? Copy : Opposite;
    }

    public static Color Of(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Reverse: return Opposite;
            case GameMode.Combo: return Combo;
            default: return Copy;
        }
    }
}
