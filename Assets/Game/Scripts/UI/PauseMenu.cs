using UnityEngine;
using UnityEngine.InputSystem;

// ESC (or the pause button) pauses the game: time stops, the pause panel opens.
// Continue / Restart / Menu buttons on the panel call the methods below.
public class PauseMenu : MonoBehaviour
{
    public GameFlow gameFlow;
    public GameManager gm;
    public PanelsBeatifulCloseOpenSmooth PausePanel;
    public PanelsBeatifulCloseOpenSmooth StartPanel;
    public GameObject PauseButton;   // small HUD button, only shown while playing

    public bool IsPaused { get; private set; }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame))
        {
            if (IsPaused) Resume();
            else Pause();
        }

        if (PauseButton != null)
        {
            bool show = gameFlow.GameIsRunning && !IsPaused;
            if (PauseButton.activeSelf != show) PauseButton.SetActive(show);
        }
    }

    public void Pause()
    {
        if (IsPaused || !gameFlow.GameIsRunning) return;   // only during a game
        IsPaused = true;
        Time.timeScale = 0f;   // stops level timers, NPC jumps, effects (menus use unscaled time)
        PausePanel.Open();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        PausePanel.Close();
    }

    public void Restart()   // same mode again from the start
    {
        bool training = gm.StartMode == GameStartingMode.Training;
        Resume();
        gameFlow.AbortGame();
        if (training) gameFlow.StartTraining();
        else gameFlow.StartGame();
    }

    public void ToMenu()
    {
        Resume();
        gameFlow.AbortGame();
        StartPanel.Open();
    }

    void OnDisable()
    {
        Time.timeScale = 1f;   // never leave the game frozen
    }
}
