using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Книжный ряд паззла как статичный собираемый объект: пока игрок не вставит «Книгу всадника» в окне сборки,
    /// книги в слоте нет, а сам паззл (BOOK_SWAP) недоступен. Complete() вешается на AssemblyInteractable.onAssemblyCompleted,
    /// ApplyCompleted() — на WorldEventSaver.onStateRestored (состояние после загрузки).
    /// </summary>
    public class BookRowGate : MonoBehaviour
    {
        [Tooltip("Книга всадника на полке (Book_9) — скрыта до сборки")]
        public GameObject riderBook;
        [Tooltip("Коллайдер паззла BOOK_SWAP")]
        public Collider puzzleCollider;
        [Tooltip("Interactable паззла BOOK_SWAP")]
        public Behaviour puzzleInteractable;
        [Tooltip("Коллайдер и Interactable этого объекта (вход в сборку) — выключаются после сборки")]
        public Collider assemblyCollider;
        public Behaviour assemblyInteractable;

        private bool _completed;

        private void Awake()
        {
            if (!_completed) SetLocked(true);
        }

        public void Complete()
        {
            ApplyCompleted();
            var saver = GetComponent<WorldEventSaver>();
            if (saver != null) saver.SaveEventState();
        }

        public void ApplyCompleted()
        {
            _completed = true;
            SetLocked(false);
        }

        private void SetLocked(bool locked)
        {
            if (riderBook) riderBook.SetActive(!locked);
            if (puzzleCollider) puzzleCollider.enabled = !locked;
            if (puzzleInteractable) puzzleInteractable.enabled = !locked;
            if (assemblyCollider) assemblyCollider.enabled = locked;
            if (assemblyInteractable) assemblyInteractable.enabled = locked;
        }
    }
}
