using System.IO;
using System.Linq;
using NightCycle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-off: OfficeSecretStatues - activateDelay 4 s, Office 1/2/6 all start from the component (no StatueBreath.Breathe),
// breath sound = Office 6's breatheEvent or, if empty, the GEAR statue's. Logs every StatueBreath event path. Log Temp/qmd_ssupd.txt.
public static class QmdSecretStatuesUpd
{
    const string SelfPath = "Assets/Editor/QmdSecretStatuesUpd.cs";
    [InitializeOnLoadMethod] static void Go() => EditorApplication.delayCall += Run;
    static string P(Transform t) => t.parent ? P(t.parent) + "/" + t.name : t.name;
    static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var log = "";
        try
        {
            var sc = EditorSceneManager.GetSceneByPath("Assets/Scenes/NightCycle.unity");
            var c = sc.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<OfficeSecretStatues>(true)).First();
            var breaths = sc.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<StatueBreath>(true)).ToList();
            foreach (var b in breaths) log += $"{P(b.transform)} event '{b.breatheEvent.Path}' null {b.breatheEvent.IsNull}\n";
            var src = !c.breather.breatheEvent.IsNull ? c.breather : breaths.FirstOrDefault(b => P(b.transform).Contains("GEAR_STATUE") && !b.breatheEvent.IsNull) ?? breaths.FirstOrDefault(b => !b.breatheEvent.IsNull);
            c.breathEvent = src ? src.breatheEvent : default;
            log += $"breath sound from {(src ? P(src.transform) : "NONE")}\n";
            if (!c.silentMovers.Contains(c.breather)) c.silentMovers.Add(c.breather);
            c.activateDelay = 4f; c.saveAfterOpen = true;
            log += "movers: " + string.Join(",", c.silentMovers.Select(s => s.name)) + "; delay 4\n";
            EditorUtility.SetDirty(c); EditorSceneManager.MarkSceneDirty(sc); EditorSceneManager.SaveScene(sc);
            log += "saved";
        }
        catch (System.Exception e) { log += "EXC " + e; }
        File.WriteAllText("Temp/qmd_ssupd.txt", log);
        AssetDatabase.DeleteAsset(SelfPath);
    }
}
