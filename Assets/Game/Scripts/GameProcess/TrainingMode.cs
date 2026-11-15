using UnityEngine;
using System.Collections;
using TMPro;

[System.Serializable]
public struct TrainingLevel
{
    [TextArea] public string Instruction;     // what the player reads on screen (English)
    [TextArea] public string InstructionRu;   // the same in Russian
    public GameMode Mode;
    public StateOfBodyParts Pose;            // the pose the NPC shows
}

// 10 simple levels that teach the controls. Each level says what to do.
// Right answer -> next level. Wrong answer -> try the same level again.
public class TrainingMode : MonoBehaviour
{
    public GameFlow gameFlow;
    public GameManager gm;
    public Npc NPC;
    public TMP_Text InstructionText;
    public PanelsBeatifulCloseOpenSmooth StartPanel;   // opens again when training is done

    public float TimePerLevel = 6f;
    public float PauseBetweenLevels = 1f;
    public float EndMessageTime = 2.5f;

    public TrainingLevel[] Levels = DefaultLevels();

    Coroutine trainingRoutine;

    void Start()
    {
        InstructionText.gameObject.SetActive(false);
    }

    public void Run()
    {
        if (trainingRoutine != null) StopCoroutine(trainingRoutine);
        trainingRoutine = StartCoroutine(TrainingLoop());
    }

    public void Abort()   // pause menu Restart / Menu
    {
        if (trainingRoutine != null) StopCoroutine(trainingRoutine);
        trainingRoutine = null;
        InstructionText.gameObject.SetActive(false);
    }

    IEnumerator TrainingLoop()
    {
        InstructionText.gameObject.SetActive(true);
        int i = 0;

        while (i < Levels.Length && gameFlow.GameIsRunning)
        {
            TrainingLevel level = Levels[i];
            gameFlow.CurrentLevel = i;

            if (gm.CurrentGameMode != level.Mode)
            {
                gm.SetGameMode(level.Mode);   // shows the "REVERSE MODE!!!" popup
            }

            string instruction = Loc.IsRussian && !string.IsNullOrEmpty(level.InstructionRu) ? level.InstructionRu : level.Instruction;
            InstructionText.text = Loc.Format("training.level", i + 1, Levels.Length) + "\n" + MirrorSettings.FixKeys(instruction);   // A/D swap with mirror controls
            NPC.SetPose(level.Pose, TimePerLevel * 0.5f);   // jumps in the middle of the level
            gameFlow.RaiseLevelStarted(TimePerLevel);
            yield return new WaitForSeconds(TimePerLevel);

            if (gm.CheckMatch())
            {
                gm.ChangheScore(1);
                InstructionText.text = Loc.Get("training.nice");
                i++;
            }
            else
            {
                gm.ChangheScore(-1);
                InstructionText.text = Loc.Get("training.retry");
            }

            yield return new WaitForSeconds(PauseBetweenLevels);
        }

        InstructionText.text = Loc.Get("training.done");
        gameFlow.StopGame();
        yield return new WaitForSeconds(EndMessageTime);

        InstructionText.gameObject.SetActive(false);
        gm.StartMode = GameStartingMode.Infinity;
        gm.CurrentGameMode = GameMode.Default;
        NPC.SetPose(new StateOfBodyParts { IsStanding = true });
        if (StartPanel != null) StartPanel.Open();
        trainingRoutine = null;
    }

    void Reset() => Levels = DefaultLevels();   // right-click component > Reset brings these back

    const string Y = "<color=#FFD040>", E = "</color>";   // yellow key highlight

    public static TrainingLevel[] DefaultLevels()
    {
        return new[]
        {
            L("Hold the pose until the timer ends.\nHe stands still - just do nothing!",
              "Держи позу, пока не кончится таймер.\nОн стоит спокойно - ничего не делай!",   GameMode.Default, false, false, true, false),
            L("He raised a hand.\nHold "+Y+"A"+E+" to copy him!",
              "Он поднял руку.\nЗажми "+Y+"A"+E+", чтобы повторить!",                       GameMode.Default, true,  false, true, false),
            L("Other hand now.\nHold "+Y+"D"+E+"!",
              "Теперь другая рука.\nЗажми "+Y+"D"+E+"!",                                    GameMode.Default, false, true,  true, false),
            L("Both hands up!\nHold "+Y+"A"+E+" + "+Y+"D"+E,
              "Обе руки вверх!\nЗажми "+Y+"A"+E+" + "+Y+"D"+E,                              GameMode.Default, true,  true,  true, false),
            L("He crouches.\nHold "+Y+"S"+E+" to crouch!",
              "Он приседает.\nЗажми "+Y+"S"+E+", чтобы присесть!",                          GameMode.Default, false, false, false, false),
            L("He jumps.\nPress "+Y+"SPACE"+E+" to jump!",
              "Он прыгает.\nНажми "+Y+"ПРОБЕЛ"+E+", чтобы прыгнуть!",                       GameMode.Default, false, false, true, true),
            L("Crouch with a hand up.\nHold "+Y+"S"+E+" + "+Y+"A"+E,
              "Присядь с поднятой рукой.\nЗажми "+Y+"S"+E+" + "+Y+"A"+E,                    GameMode.Default, true, false, false, false),
            L("REVERSE: do the opposite!\nHe stands with one hand up -\nhold "+Y+"D"+E+" + "+Y+"S"+E,
              "НАОБОРОТ: делай противоположное!\nОн стоит с поднятой рукой -\nзажми "+Y+"D"+E+" + "+Y+"S"+E, GameMode.Reverse, true, false, true, false),
            L("Opposite again!\nHe crouches with hands down -\nstand and hold "+Y+"A"+E+" + "+Y+"D"+E,
              "Снова наоборот!\nОн сидит с опущенными руками -\nвстань и зажми "+Y+"A"+E+" + "+Y+"D"+E, GameMode.Reverse, false, false, false, false),
            L("He jumps - so you DON'T!\nHold "+Y+"A"+E+" + "+Y+"D"+E+", no jumping",
              "Он прыгает - а ты НЕТ!\nЗажми "+Y+"A"+E+" + "+Y+"D"+E+" и не прыгай",          GameMode.Reverse, false, false, true, true),
        };
    }

    static TrainingLevel L(string text, string textRu, GameMode mode, bool left, bool right, bool standing, bool jumped)
    {
        return new TrainingLevel
        {
            Instruction = text,
            InstructionRu = textRu,
            Mode = mode,
            Pose = new StateOfBodyParts { LeftHandIsUp = left, RightHandIsUp = right, IsStanding = standing, Jumped = jumped }
        };
    }
}
