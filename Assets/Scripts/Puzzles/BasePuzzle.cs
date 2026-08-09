using UnityEngine;
using UnityEngine.Events;

namespace NightCycle.Puzzles
{
    public abstract class BasePuzzle : MonoBehaviour
    {
        [Tooltip("Уникальный ID для сохранения")]
        public string puzzleId;
        public bool isSolved;
        public UnityEvent onPuzzleSolved;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(puzzleId))
            {
                puzzleId = System.Guid.NewGuid().ToString();
            }
        }

        public virtual void SolvePuzzle()
        {
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