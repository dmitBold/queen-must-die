using System.IO;
using System.Linq;
using NightCycle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Temporary play test (NightCycle): forces the Statues branch (Office_statues + spider_figure_alt on), solves the book puzzle from
// code, logs the timeline (swap, breath, activation), then fires the catacombs teleport hook and logs the vanish state. Log Temp/qmd_sstest2.txt.
[InitializeOnLoad]
public static class QmdSecretStatuesTest2
{
    const string SelfPath = "Assets/Editor/QmdSecretStatuesTest2.cs", Key = "QmdSecretStatuesTest2.stage";
    static double _t0, _tSolve; static int _step;
    static QmdSecretStatuesTest2()
    {
        int st = SessionState.GetInt(Key, 0);
        if (st == 0 && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SessionState.SetInt(Key, 1);
            EditorApplication.delayCall += () =>
            {
                if (Enumerable.Range(0, EditorSceneManager.sceneCount).Any(i => EditorSceneManager.GetSceneAt(i).isDirty)) EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene("Assets/Scenes/NightCycle.unity", OpenSceneMode.Single);
                EditorApplication.EnterPlaymode();
            };
        }
        else if (st == 1 && EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.update += Tick;
        else if (st == 2 && !EditorApplication.isPlayingOrWillChangePlaymode) Finish();
    }
    static void Log(string s) => File.AppendAllText("Temp/qmd_sstest2.txt", s + "\n");
    static GameObject G(string path) { var parts = path.Split('/'); var root = UnityEngine.SceneManagement.SceneManager.GetSceneByName("NightCycle").GetRootGameObjects().First(r => r.name == parts[0]).transform; return (parts.Length == 1 ? root : root.Find(string.Join("/", parts.Skip(1)))).gameObject; }
    static string State(OfficeSecretStatues c)
    {
        var o6 = G("Office_statues/ActiveStatues/Office 6"); var o1 = G("Office_statues/ActiveStatues/Office 1");
        var a6 = o6.GetComponent<UnityEngine.AI.NavMeshAgent>(); var a1 = o1.GetComponent<UnityEngine.AI.NavMeshAgent>();
        var st = c.secretAnimator.GetCurrentAnimatorStateInfo(0);
        return $"static {c.staticStatues.activeSelf} active {c.activeStatues.activeSelf}; atack {G("Office_statues/Atack trigger").activeSelf} stop {G("Office_statues/Statues stop trig").activeSelf}; " +
               $"O6 agent dest {(a6.isActiveAndEnabled && a6.hasPath)} vel {(a6.isActiveAndEnabled ? a6.velocity.magnitude : 0):F2}; O1 hasPath {(a1.isActiveAndEnabled && a1.hasPath)}; secret state {(st.IsName(c.openStateName) ? "open " + st.normalizedTime.ToString("F2") : "idle")} root {c.secretRoot.activeSelf}; " +
               $"fig default {c.figureDefault.activeSelf} alt {c.figureAlt.activeSelf}; puzzle col {c.puzzleCollider.enabled}";
    }
    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        if (_t0 == 0) _t0 = EditorApplication.timeSinceStartup;
        double t = EditorApplication.timeSinceStartup - _t0;
        try
        {
            var c = Object.FindFirstObjectByType<OfficeSecretStatues>();
            if (_step == 0 && t > 12)
            {
                G("Office_statues").SetActive(true); G("PEDESTAL QUEST/Figures/spider_figure_alt").SetActive(true); G("PEDESTAL QUEST/Figures/spider_figure").SetActive(false);
                Log("branch forced: " + State(c));
                G("PUZZLES/BooksPuzzle/Secret enter/BOOK_SWAP").GetComponent<NightCycle.Puzzles.SwapPuzzle>().SolvePuzzle();
                _tSolve = t; _step = 1; Log("solved at 0");
            }
            else if (_step >= 1 && _step <= 6 && t > _tSolve + 2.0 * _step)
            { Log($"+{t - _tSolve:F1}s: " + State(c)); _step++; }
            else if (_step == 7 && t > _tSolve + 16)
            {
                Log("+6s: " + State(c));
                c.OnTeleported(); Log("teleported");
                _step = 8;
            }
            else if (_step == 8 && t > _tSolve + 17)
            {
                Log("+7s after teleport: " + State(c));
                _step = 9; EditorApplication.update -= Tick; SessionState.SetInt(Key, 2); EditorApplication.delayCall += () => EditorApplication.ExitPlaymode();
            }
        }
        catch (System.Exception e) { Log("EXC " + e); _step = 99; EditorApplication.update -= Tick; SessionState.SetInt(Key, 2); EditorApplication.delayCall += () => EditorApplication.ExitPlaymode(); }
    }
    static void Finish() { SessionState.EraseInt(Key); EditorApplication.update -= Tick; AssetDatabase.DeleteAsset(SelfPath); }
}
