using UnityEngine;
using TMPro;

// Small menu button: shows A|D (normal) or D|A (mirror controls, keys swapped) and switches it.
public class MirrorButton : MonoBehaviour
{
    public TMP_Text Label;

    void OnEnable()
    {
        MirrorSettings.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        MirrorSettings.OnChanged -= Refresh;
    }

    public void Toggle() => MirrorSettings.Toggle();

    void Refresh()
    {
        if (Label != null) Label.text = MirrorSettings.SwapKeys ? "D|A" : "A|D";
    }
}
