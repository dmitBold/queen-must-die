using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Сначала объект показывает диалог (NightDialogueInteractable), после его прочтения диалог убирается
    /// и взаимодействие идёт в обычный Interactable.onInteraction (например, сборка голема).
    /// Повесить Unlock() в NightDialogueInteractable.onDialogueCompleted. Состояние сохраняется через SaveSystem.
    /// </summary>
    public class DialogueGate : MonoBehaviour
    {
        public NightDialogueInteractable dialogue;
        [Tooltip("Уникальный id для сохранения")]
        public string saveId;

        private SaveSystem _saveSystem;

        [Inject]
        public void Construct(SaveSystem saveSystem) => _saveSystem = saveSystem;

        private void Start()
        {
            if (_saveSystem != null && !string.IsNullOrEmpty(saveId) && _saveSystem.SessionTriggeredEvents.Contains(saveId)) RemoveDialogue();
        }

        public void Unlock()
        {
            if (_saveSystem != null && !string.IsNullOrEmpty(saveId)) _saveSystem.RegisterEvent(saveId);
            RemoveDialogue();
        }

        private void RemoveDialogue()
        {
            if (dialogue) Destroy(dialogue);
            dialogue = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(saveId)) { saveId = System.Guid.NewGuid().ToString(); UnityEditor.EditorUtility.SetDirty(this); }
        }
#endif
    }
}
