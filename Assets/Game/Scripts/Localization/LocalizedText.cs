using UnityEngine;
using TMPro;

// Put on any TMP label that never changes from code. Shows Loc text for Key and updates when language changes.
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string Key;

    TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    void OnEnable()
    {
        Loc.OnLanguageChanged += Refresh;
        MirrorSettings.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        Loc.OnLanguageChanged -= Refresh;
        MirrorSettings.OnChanged -= Refresh;
    }

    void Refresh()
    {
        // FixKeys: with mirror controls on, the A/D key letters in the text swap too
        if (!string.IsNullOrEmpty(Key)) text.text = MirrorSettings.FixKeys(Loc.Get(Key));
    }
}
