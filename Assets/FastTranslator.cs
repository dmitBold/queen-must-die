using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FastTranslator : EditorWindow
{
    private static string exportPath = "Assets/Translation.tsv";
    private static Regex russianRegex = new Regex(@"[А-Яа-яЁё]");

    [MenuItem("Tools/Экстренный Перевод/1. Выгрузить русский текст")]
    public static void ExportText()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string startScene = EditorSceneManager.GetActiveScene().path;
        HashSet<string> uniqueStrings = new HashSet<string>();

        ProcessAllAssets("Сбор текста", prop =>
        {
            string text = prop.stringValue.Replace("\r", "");
            if (!string.IsNullOrEmpty(text) && russianRegex.IsMatch(text))
            {
                uniqueStrings.Add(text);
            }
        });

        using (StreamWriter writer = new StreamWriter(exportPath, false, System.Text.Encoding.UTF8))
        {
            writer.WriteLine("Original\tTranslated");
            foreach (string str in uniqueStrings)
            {
                string safeStr = str.Replace("\n", "[NEWLINE]");
                writer.WriteLine($"{safeStr}\t");
            }
        }

        AssetDatabase.Refresh();
        RestoreScene(startScene);
        Debug.Log($"[Export] ГОТОВО! Найдено уникальных строк: {uniqueStrings.Count}.");
    }

    [MenuItem("Tools/Экстренный Перевод/2. Загрузить и заменить на английский")]
    public static void ImportText()
    {
        if (!File.Exists(exportPath))
        {
            Debug.LogError("Файл перевода не найден!");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string startScene = EditorSceneManager.GetActiveScene().path;
        Dictionary<string, string> translationDict = new Dictionary<string, string>();
        string[] lines = File.ReadAllLines(exportPath);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split('\t');
            if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                string original = CleanExcelFormatting(parts[0]).Replace("[NEWLINE]", "\n");
                string translated = CleanExcelFormatting(parts[1]).Replace("[NEWLINE]", "\n");
                translationDict[original] = translated;
            }
        }

        Debug.Log($"[Import] Загружено переводов из файла: {translationDict.Count}");

        int replacedCount = 0;
        ProcessAllAssets("Замена текста", prop =>
        {
            string text = prop.stringValue.Replace("\r", "");

            if (translationDict.TryGetValue(text, out string translatedText))
            {
                prop.stringValue = translatedText;
                replacedCount++;
            }
        });

        RestoreScene(startScene);
        Debug.Log($"[Import] ГОТОВО! Успешно заменено {replacedCount} строк.");
    }

    private static void RestoreScene(string scenePath)
    {
        if (!string.IsNullOrEmpty(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    private static string CleanExcelFormatting(string input)
    {
        if (input.StartsWith("\"") && input.EndsWith("\"") && input.Length > 1)
        {
            return input.Substring(1, input.Length - 2).Replace("\"\"", "\"");
        }
        return input;
    }

    private static void ProcessAllAssets(string title, System.Action<SerializedProperty> action)
    {
        // Вместо поиска по типам, берем вообще все файлы проекта
        string[] allPaths = AssetDatabase.GetAllAssetPaths();
        List<string> pathsToProcess = new List<string>();

        foreach (string p in allPaths)
        {
            if (p.StartsWith("Assets/"))
            {
                // Фильтруем только те файлы, где может лежать нужный нам текст
                if (p.EndsWith(".prefab") || p.EndsWith(".unity") || p.EndsWith(".asset") || p.EndsWith(".playable"))
                {
                    pathsToProcess.Add(p);
                }
            }
        }

        int total = pathsToProcess.Count;
        int current = 0;

        try
        {
            foreach (string path in pathsToProcess)
            {
                current++;
                if (current % 5 == 0) // Обновляем UI не каждый кадр, чтобы не тормозить
                {
                    EditorUtility.DisplayProgressBar(title, $"Обработка: {path}", (float)current / total);
                }

                try
                {
                    if (path.EndsWith(".prefab"))
                    {
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (obj != null)
                        {
                            bool modified = false;
                            ProcessGameObjectTree(obj, action, ref modified);
                            if (modified) PrefabUtility.SavePrefabAsset(obj);
                        }
                    }
                    else if (path.EndsWith(".unity"))
                    {
                        Scene scene = EditorSceneManager.OpenScene(path);
                        bool sceneModified = false;

                        GameObject[] roots = scene.GetRootGameObjects();
                        foreach (GameObject root in roots)
                        {
                            ProcessGameObjectTree(root, action, ref sceneModified);
                        }

                        if (sceneModified)
                        {
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                        }
                    }
                    else // .asset (ScriptableObjects) и .playable (Timeline)
                    {
                        Object[] allObjs = AssetDatabase.LoadAllAssetsAtPath(path);
                        bool assetModified = false;

                        foreach (Object obj in allObjs)
                        {
                            if (obj == null) continue;
                            try
                            {
                                SerializedObject so = new SerializedObject(obj);
                                if (ProcessSerializedObject(so, action))
                                {
                                    EditorUtility.SetDirty(obj);
                                    assetModified = true;
                                }
                            }
                            catch { /* Игнорируем объекты, которые нельзя сериализовать */ }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[FastTranslator] Пропущен файл {path} из-за ошибки: {ex.Message}");
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }
    }

    private static void ProcessGameObjectTree(GameObject rootObject, System.Action<SerializedProperty> action, ref bool wasModified)
    {
        if (rootObject == null) return;

        Transform[] transforms = rootObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in transforms)
        {
            if (t == null || t.gameObject == null) continue;

            // 1. Проверяем само название GameObject
            try
            {
                SerializedObject soGo = new SerializedObject(t.gameObject);
                if (ProcessSerializedObject(soGo, action))
                {
                    wasModified = true;
                    EditorUtility.SetDirty(t.gameObject);
                }
            }
            catch { }

            // 2. Проверяем все компоненты на этом объекте
            Component[] components = t.gameObject.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null) continue; // Защита от Missing Script
                try
                {
                    SerializedObject soComp = new SerializedObject(comp);
                    if (ProcessSerializedObject(soComp, action))
                    {
                        wasModified = true;
                        EditorUtility.SetDirty(comp);
                    }
                }
                catch { }
            }
        }
    }

    private static bool ProcessSerializedObject(SerializedObject so, System.Action<SerializedProperty> action)
    {
        if (so == null) return false;

        bool modified = false;
        SerializedProperty prop = so.GetIterator();
        bool enterChildren = true;

        while (prop.Next(enterChildren))
        {
            enterChildren = true;
            if (prop.propertyType == SerializedPropertyType.String)
            {
                string before = prop.stringValue;
                action(prop);
                if (before != prop.stringValue)
                {
                    modified = true;
                }
            }
        }

        if (modified)
        {
            so.ApplyModifiedProperties();
        }
        return modified;
    }
}