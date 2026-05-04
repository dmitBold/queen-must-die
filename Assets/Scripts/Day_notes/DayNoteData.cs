using UnityEngine;
using System.Collections.Generic;
using Zenject;

namespace Inventory
{
    [CreateAssetMenu(fileName = "DayNoteData", menuName = "Scriptable Objects/DayNoteData")]
    public class DayNoteData : ScriptableObject
    {
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