using UnityEditor;

// Temporary leftover remover: deletes itself.
public static class QmdSceneUsageProbeStub
{
    [InitializeOnLoadMethod] static void Schedule() { EditorApplication.update += Tick; }
    static void Tick() { EditorApplication.update -= Tick; AssetDatabase.DeleteAsset("Assets/Editor/QmdSceneUsageProbe.cs"); }
}
