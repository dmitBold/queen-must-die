using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Temporary play test: opens DayCycle, enters play mode, after 14 s shows a set of Day 1 texts through the real DialogueController
// and captures the Game view to Temp/qmd_dayshot_N.png (N=0 is the stats bar + first card). Exits play mode, deletes itself.
[InitializeOnLoad]
public static class QmdDayShots3
{
    const string SelfPath = "Assets/Editor/QmdDayShots3.cs";
    const string Key = "QmdDayShots3.stage";
    static readonly (string name, string text)[] Texts = {
        ("Наместница пауков", "Александр дрожал за спиной сестры паучьего вождя. «Отвлеки его, я с детства боюсь пауков»."),
        ("Наместница пауков", "Мне запретить его?"),
        ("Сотер Птолемей", "Вы, конечно, в курсе, что от ос избавляются огнём."),
        ("Сотер Птолемей", "При возможности предупредите наместницу, чтобы мы не жгли их живьём. Опять."),
        ("Генерал Гефестион", "После коронации нового Аргеада те, кто правят от его имени на местах..."),
        ("Наместница ос", "«Книга всадника открывает их тайны». Так он мне объяснил свой дар."),
        
    };
    static double _t0; static int _shot = -1; static double _next;
    static QmdDayShots3()
    {
        int st = SessionState.GetInt(Key, 0);
        if (st == 0 && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SessionState.SetInt(Key, 1);
            EditorApplication.delayCall += () =>
            {
                bool dirty = Enumerable.Range(0, EditorSceneManager.sceneCount).Any(i => EditorSceneManager.GetSceneAt(i).isDirty);
                if (dirty) { var a = EditorSceneManager.GetActiveScene(); if (EditorSceneManager.sceneCount == 1 && a.path == "Assets/Scenes/DayCycle.unity") EditorSceneManager.SaveScene(a); else { File.WriteAllText("Temp/qmd_dayshots.txt", "unsaved scenes - abort"); Finish(); return; } }
                EditorSceneManager.OpenScene("Assets/Scenes/DayCycle.unity", OpenSceneMode.Single);
                EditorApplication.EnterPlaymode();
            };
        }
        else if (st == 1 && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.update += Tick;
        }
        else if (st == 2 && !EditorApplication.isPlayingOrWillChangePlaymode) Finish();
    }
    static bool _shown;
    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        if (_t0 == 0) { _t0 = EditorApplication.timeSinceStartup; _next = _t0 + 14; }
        if (EditorApplication.timeSinceStartup < _next) return;
        if (_shown) { ScreenCapture.CaptureScreenshot($"Temp/qmd_dayshot_{_shot}.png"); _shown = false; _next = EditorApplication.timeSinceStartup + 1; return; }
        _shot++;
        if (_shot >= Texts.Length) { EditorApplication.update -= Tick; SessionState.SetInt(Key, 2); EditorApplication.delayCall += () => EditorApplication.ExitPlaymode(); return; }
        Object.FindFirstObjectByType<Dialogue.DialogueController>().Show(new[] { Texts[_shot].text }, Texts[_shot].name);
        _shown = true; _next = EditorApplication.timeSinceStartup + 7;
    }
    static void Finish() { SessionState.EraseInt(Key); EditorApplication.update -= Tick; AssetDatabase.DeleteAsset(SelfPath); }
}
