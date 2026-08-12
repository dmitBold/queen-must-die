using System.Collections.Generic;
using UnityEngine;

namespace NightCycle.Puzzles
{
    public class RotationPuzzle : OpenPuzzle
    {
        [Header("Puzzle Elements")]
        [SerializeField] private List<RotatableItem> puzzleItems;

        private void Start()
        {
            // Подписываемся на завершение вращения каждого предмета
            foreach (var item in puzzleItems)
            {
                item.OnRotationFinished += CheckWinCondition;
            }

            LoadPuzzleState();
        }

        private void OnDestroy()
        {
            // Отписываемся, чтобы избежать утечек памяти
            foreach (var item in puzzleItems)
            {
                if (item != null) item.OnRotationFinished -= CheckWinCondition;
            }
        }

        private void CheckWinCondition()
        {
            if (isSolved) return; // Унаследовано от BasePuzzle

            foreach (var item in puzzleItems)
            {
                if (!item.IsAtTargetAngle())
                {
                    return; // Если хотя бы один не на месте - паззл не решен
                }
            }

            // Если цикл прошел до конца, значит все предметы стоят правильно
            SolvePuzzle(); // Унаследовано от BasePuzzle, вызовет onPuzzleSolved.Invoke()
        }

        protected override void SavePuzzleResult()
        {
            // Сохранение факта решения (WorldEventSaver)
            var eventSaver = GetComponent<WorldEventSaver>();
            if (eventSaver != null) eventSaver.SaveEventState();
        }

        public override void LoadPuzzleState()
        {
            // Здесь вытаскиваем CurrentAngle иSaveManager для каждого RotatableItem
            // и вызываем item.SetAngleInstantly(savedAngle);
        }
    }
}