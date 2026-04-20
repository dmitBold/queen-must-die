using UnityEditor;
using UnityEngine;

namespace NightCycle.Editor
{
    /// <summary>
    /// Набор утилит для управления PlayerPrefs прямо из редактора Unity.
    /// Меню: Tools → PlayerPrefs
    /// </summary>
    public static class PlayerPrefsTools
    {
        private const string BrightnessKey = "BrightnessSetting";

        // ─── Удалить ВСЕ PlayerPrefs ────────────────────────────────────────────

        [MenuItem("Tools/PlayerPrefs/Delete ALL PlayerPrefs", priority = 0)]
        private static void DeleteAll()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                title:   "Удалить все PlayerPrefs?",
                message: "Это действие необратимо. Все сохранённые данные (настройки, прогресс и т.д.) будут удалены.",
                ok:      "Удалить",
                cancel:  "Отмена"
            );

            if (!confirmed) return;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[PlayerPrefsTools] Все PlayerPrefs удалены.");
        }


        // ─── Удалить только настройку яркости ───────────────────────────────────

        [MenuItem("Tools/PlayerPrefs/Delete Brightness Setting", priority = 1)]
        private static void DeleteBrightness()
        {
            if (!PlayerPrefs.HasKey(BrightnessKey))
            {
                Debug.Log($"[PlayerPrefsTools] Ключ '{BrightnessKey}' не найден — нечего удалять.");
                return;
            }

            PlayerPrefs.DeleteKey(BrightnessKey);
            PlayerPrefs.Save();
            Debug.Log($"[PlayerPrefsTools] Ключ '{BrightnessKey}' удалён.");
        }


        // ─── Показать текущие значения (отладка) ────────────────────────────────

        [MenuItem("Tools/PlayerPrefs/Log Brightness Value", priority = 2)]
        private static void LogBrightness()
        {
            if (PlayerPrefs.HasKey(BrightnessKey))
                Debug.Log($"[PlayerPrefsTools] {BrightnessKey} = {PlayerPrefs.GetFloat(BrightnessKey):F3}");
            else
                Debug.Log($"[PlayerPrefsTools] Ключ '{BrightnessKey}' не найден (будет использовано значение по умолчанию).");
        }
    }
}
