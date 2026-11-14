using UnityEngine;
using System.Collections;

public class GameFlow : MonoBehaviour
{
    public StateOfBodyParts[] levels;
    public Npc NPC;
    public GameManager gm;
    public float _TimePerLevel;

    public int CurrentLevel;  
    public int Score;
    public bool GameIsRunning;

    void Start()
    {
        GameIsRunning = true;
        StartCoroutine(LevelsTimer());
    }

    public IEnumerator LevelsTimer()
    {
        CurrentLevel = 0;

        while (GameIsRunning && CurrentLevel < levels.Length)
        {
            NPC.SetPose(levels[CurrentLevel]);
            yield return new WaitForSeconds(_TimePerLevel);

            if (gm.CheckMatch())
            {
                Score++;
            }
            else
            {
                Score--;
            }

            CurrentLevel++;
        }

        // all poses are done
        GameIsRunning = false;
        Debug.Log("Game over. Score: " + Score);
    }
}
