using UnityEditor;

public static class FreezeCompilation
{
    [MenuItem("Tools/Freeze Compilation")]
    public static void Freeze()
    {
        EditorApplication.LockReloadAssemblies();
    }

    [MenuItem("Tools/Unfreeze Compilation")]
    public static void Unfreeze()
    {
        EditorApplication.UnlockReloadAssemblies();
        AssetDatabase.Refresh();
    }
}
