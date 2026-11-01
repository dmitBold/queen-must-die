using UnityEngine;
using System.Collections.Generic;
using Zenject;

namespace Inventory
{
    [CreateAssetMenu(fileName = "DayNoteData", menuName = "Scriptable Objects/DayNoteData")]
    public class DayNoteData : ScriptableObject
    {
        [Header("Save")]
        [Tooltip("Уникальный id для сохранения. Добавьте записку в DayNoteDatabase, иначе она не восстановится после загрузки.")]
        public string id;

        [Header("Note Pages")]
        [TextArea(3, 5)]
        public string[] NotePages;

        [Header("Note Name")]
        [TextArea]
        public string NoteName;

        [Header("Visual")]
        public Sprite icon;

    }
}