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

    // ЧЕРНЫЙ СПИСОК: Эти системные поля Unity переводить НЕЛЬЗЯ.
    // Если у тебя в скриптах есть строковые переменные для триггеров аниматора 
    // (например, public string attackTrigger;), впиши сюда их точные названия!
    private static readonly HashSet<string> ignoredFields = new HashSet<string>
    {
        "m_Name",             // Имена GameObject, компонентов и ассетов
        "m_MethodName",       // Названия методов в UnityEvents (кнопки UI и т.д.)
        "m_TagString",        // Теги
        "m_LayerName",        // Слои
        "m_SortingLayerName", // Слои сортировки
        "m_FontWeight",       // Настройки шрифта
        "m_TargetPort"        // Системные строки Timeline
    };

    [MenuItem("Tools/Экстренный Перевод/1. Выгрузить русский текст")]
    public static void ExportText()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Запоминаем все открытые вкладки сцен, чтобы вернуть всё как было
        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();
        HashSet<string> uniqueStrings = new HashSet<string>();

        ProcessAllAssets("Сбор текста", prop =>
        {
            string text = prop.stringValue.Replace("\r", "");
            if (!string.IsNullOrEmpty(text) && russianRegex.IsMatch(text))
            {
                uniqueStrings.Add(text);
            }
        });

        // Используем UTF8 с BOM (true), чтобы Excel 100% правильно понял кириллицу
        using (StreamWriter writer = new StreamWriter(exportPath, false, new System.Text.UTF8Encoding(true)))
        {
            writer.WriteLine("Original\tTranslated");
            foreach (string str in uniqueStrings)
            {
                // Защита структуры TSV файла от символов внутри текста
                string safeStr = str.Replace("\n", "[NEWLINE]").Replace("\t", "[TAB]");
                writer.WriteLine($"{safeStr}\t");
            }
        }

        AssetDatabase.Refresh();
        RestoreWorkspace(sceneSetup);
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

        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();
        Dictionary<string, string> translationDict = new Dictionary<string, string>();
        string[] lines = File.ReadAllLines(exportPath, System.Text.Encoding.UTF8);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] parts = lines[i].Split('\t');
            if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                string original = CleanExcelFormatting(parts[0]).Replace("[NEWLINE]", "\n").Replace("[TAB]", "\t");
                string translated = CleanExcelFormatting(parts[1]).Replace("[NEWLINE]", "\n").Replace("[TAB]", "\t");
                translationDict[original] = translated;
            }
        }

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

        RestoreWorkspace(sceneSetup);
        Debug.Log($"[Import] ГОТОВО! Успешно заменено {replacedCount} строк.");
    }

    private static void RestoreWorkspace(SceneSetup[] setup)
    {
        if (setup != null && setup.Length > 0)
        {
            EditorSceneManager.RestoreSceneManagerSetup(setup);
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
        string[] allPaths = AssetDatabase.GetAllAssetPaths();
        List<string> pathsToProcess = new List<string>();

        foreach (string p in allPaths)
        {
            if (p.StartsWith("Assets/"))
            {
                string lowerPath = p.ToLower();
                if (lowerPath.EndsWith(".prefab") || lowerPath.EndsWith(".unity") ||
                    lowerPath.EndsWith(".asset") || lowerPath.EndsWith(".playable"))
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
                if (current % 10 == 0)
                {
                    EditorUtility.DisplayProgressBar(title, $"Обработка: {path}", (float)current / total);
                }

                try
                {
                    if (path.ToLower().EndsWith(".prefab"))
                    {
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (obj != null)
                        {
                            bool modified = false;
                            ProcessGameObjectTree(obj, action, ref modified);
                            if (modified) PrefabUtility.SavePrefabAsset(obj);
                        }
                    }
                    else if (path.ToLower().EndsWith(".unity"))
                    {
                        // Открываем в Single режиме для очистки памяти между сценами
                        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        bool sceneModified = false;

                        // Бробойный метод: берем ВСЕ объекты сцены, даже скрытые от иерархии
                        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                        foreach (GameObject obj in allObjects)
                        {
                            if (obj.scene == scene)
                            {
                                ProcessGameObject(obj, action, ref sceneModified);
                            }
                        }

                        if (sceneModified)
                        {
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                        }
                    }
                    else // .asset и .playable (включая Timeline Clips)
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
                            catch { }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[FastTranslator] Ошибка в файле {path}: {ex.Message}");
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
        Transform[] transforms = rootObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in transforms)
        {
            if (t != null && t.gameObject != null)
            {
                ProcessGameObject(t.gameObject, action, ref wasModified);
            }
        }
    }

    private static void ProcessGameObject(GameObject obj, System.Action<SerializedProperty> action, ref bool wasModified)
    {
        try
        {
            SerializedObject soGo = new SerializedObject(obj);
            if (ProcessSerializedObject(soGo, action))
            {
                wasModified = true;
                EditorUtility.SetDirty(obj);
            }
        }
        catch { }

        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp == null) continue;
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

    private static bool ProcessSerializedObject(SerializedObject so, System.Action<SerializedProperty> action)
    {
        if (so == null) return false;
        bool modified = false;
        SerializedProperty prop = so.GetIterator();
        bool enterChildren = true;

        while (prop.Next(enterChildren))
        {
            enterChildren = true;

            // Пропускаем технические поля, чтобы не ломать игру
            if (prop.propertyType == SerializedPropertyType.String && !ignoredFields.Contains(prop.name))
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