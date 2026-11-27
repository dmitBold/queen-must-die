using Cards;
using UnityEngine;
using Zenject;

namespace Core
{
    /// <summary>
    /// Переносит флаги решений доков из локального StateManager сцены в общий WorldState.
    /// Доки в King block Day намеренно пишут в локальное состояние, которое не переживает смену сцены и не сохраняется,
    /// а последствия решений нужны в других сценах (Cave Day 1).
    /// </summary>
    public class DocsFlagTransfer : MonoBehaviour
    {
        /// <summary>CardManager доков, после каждого решения которого проверяются флаги.</summary>
        [SerializeField] private CardManager _cardManager;

        /// <summary>StateManager, в который пишет CardManager доков (его поле World State Manual).</summary>
        [SerializeField] private WorldState _docsWorldState;

        /// <summary>Флаги из Add Flags выборов доков, которые нужно перенести. Должны совпадать с картами.</summary>
        [SerializeField] private string[] _flags;

        private WorldState _worldState;

        [Inject]
        public void Construct(WorldState worldState)
        {
            _worldState = worldState;
        }

        private void OnEnable()
        {
            if (_cardManager == null || _docsWorldState == null)
            {
                Debug.LogError($"{name}: не назначены CardManager или StateManager доков, решения не перенесутся.", this);
                return;
            }

            _cardManager.OnAnyChoiceResolved += TransferFlags;
        }

        private void OnDisable()
        {
            if (_cardManager != null) _cardManager.OnAnyChoiceResolved -= TransferFlags;
        }

        private void TransferFlags()
        {
            if (_worldState == null) return;

            foreach (string flag in _flags)
            {
                if (_docsWorldState.HasFlag(flag)) _worldState.AddFlag(flag);
            }
        }
    }
}
