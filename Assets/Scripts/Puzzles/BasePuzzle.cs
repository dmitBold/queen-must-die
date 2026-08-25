using Core;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle.Puzzles
{
    public abstract class BasePuzzle : MonoBehaviour
    {
        [Tooltip("Уникальный ID для сохранения")]
        public string puzzleId;
        public bool isSolved;
        public UnityEvent onPuzzleSolved;
        public EventReference PuzzleCompletedEvent;
        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(puzzleId))
            {
                puzzleId = System.Guid.NewGuid().ToString();
            }
        }

        public virtual void SolvePuzzle()
        {
            _audioService.PlayFMODEvent(PuzzleCompletedEvent, gameObject.transform.position);
            if (isSolved) return;
            isSolved = true;
            onPuzzleSolved?.Invoke();
            SavePuzzleResult();
        }

        // Сохранение самого факта решения
        protected abstract void SavePuzzleResult();

        // Загрузка состояния
        public abstract void LoadPuzzleState();
    }
}