using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Temporary play test: NightCycle, 10 s after start logs the quest state, then golem first -> door -> assembled
// (calls NightSecretsQuest methods directly), logging the stage after each. Exits play mode, deletes itself. Log Temp/qmd_nqtest.txt.
[InitializeOnLoad]
public static class QmdNightQuestTest
{
    const string SelfPath = "Assets/Editor/QmdNightQuestTest.cs", Key = "QmdNightQuestTest.stage";
    static double _t0; static int _step;
    static QmdNightQuestTest()
    {
        int st = SessionState.GetInt(Key, 0);
        if (st == 0 && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SessionState.SetInt(Key, 1);
            EditorApplication.delayCall += () =>
            {
                if (Enumerable.Range(0, EditorSceneManager.sceneCount).Any(i => EditorSceneManager.GetSceneAt(i).isDirty)) { File.WriteAllText("Temp/qmd_nqtest.txt", "unsaved scenes - abort"); Finish(); return; }
                EditorSceneManager.OpenScene("Assets/Scenes/NightCycle.unity", OpenSceneMode.Single);
                EditorApplication.EnterPlaymode();
            };
        }
        else if (st == 1 && EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.update += Tick;
        else if (st == 2 && !EditorApplication.isPlayingOrWillChangePlaymode) Finish();
    }
    static string State(NightSecretsQuest nq)
    {
        var qm = (QuestManager)typeof(NightSecretsQuest).GetField("_quests", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(nq);
        if (qm == null) return "NO QuestManager injected";
        var p = qm.GetActiveQuests().Find(q => q.Quest == nq.quest);
        return p == null ? "quest NOT active" : $"stage {p.CurrentStageIndex} '{p.GetCurrentStage()?.StageDescription}' completed={p.IsCompleted}; active quests: {string.Join(", ", qm.GetActiveQuests().Select(q => q.Quest.QuestName))}";
    }
    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        if (_t0 == 0) _t0 = EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup - _t0 < 10 + _step) return;
        var nq = Object.FindFirstObjectByType<NightSecretsQuest>();
        string line;
        if (!nq) line = "NightSecretsQuest not found";
        else switch (_step)
        {
            case 0: line = "start: " + State(nq); break;
            case 1: nq.OnGolemExamined(); line = "golem first: " + State(nq); break;
            case 2: nq.OnDoorReached(); line = "door after golem: " + State(nq); break;
            case 3: nq.OnGolemAssembled(); line = "assembled: " + State(nq); break;
            default: line = null; break;
        }
        if (line != null) File.AppendAllText("Temp/qmd_nqtest.txt", line + "\n");
        _step++;
        if (_step > 3 || !nq) { EditorApplication.update -= Tick; SessionState.SetInt(Key, 2); EditorApplication.delayCall += () => EditorApplication.ExitPlaymode(); }
    }
    static void Finish() { SessionState.EraseInt(Key); EditorApplication.update -= Tick; AssetDatabase.DeleteAsset(SelfPath); }
}
