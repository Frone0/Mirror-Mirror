using UnityEngine;

// Start menu button: switches between English and Russian. Its label uses LocalizedText "menu.language".
public class LanguageButton : MonoBehaviour
{
    public void Toggle() => Loc.Toggle();
}
