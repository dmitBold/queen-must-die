using UnityEngine;
using Zenject;

namespace UI
{
    /// <summary>
    /// Мост между UnityEvent/инспектором в сцене и UIManager из ProjectContext (как SaveLoadBridge для сохранений).
    /// Всё, что этот компонент заблокировал или спрятал, он снимает сам при уничтожении, чтобы выгрузка сцены
    /// посреди катсцены не оставила UI заблокированным.
    /// </summary>
    public class UIBridge : MonoBehaviour
    {
        private UIManager _uiManager;

        [Inject]
        private void Construct(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        private void OnDestroy()
        {
            if (_uiManager == null)
                return;

            _uiManager.UnblockAll(this);
            _uiManager.ShowAll(this);
        }

        /// <summary>
        /// Открывает окно, закрывая текущее.
        /// </summary>
        public void OpenScreen(UIScreenId id) => _uiManager.Open(id);

        /// <summary>
        /// Закрывает окно, если оно открыто.
        /// </summary>
        public void CloseScreen(UIScreenId id) => _uiManager.Close(id);

        /// <summary>
        /// Закрывает окно, если открыто, иначе открывает.
        /// </summary>
        public void ToggleScreen(UIScreenId id) => _uiManager.Toggle(id);

        /// <summary>
        /// Закрывает текущее окно.
        /// </summary>
        public void CloseCurrent() => _uiManager.CloseCurrent();

        /// <summary>
        /// Прячет все зарегистрированные оверлеи (HUD, подсказки, фонарик).
        /// </summary>
        public void HideAll() => _uiManager.HideAll(this);

        /// <summary>
        /// Снимает скрытие, поставленное этим мостом.
        /// </summary>
        public void ShowAll() => _uiManager.ShowAll(this);

        /// <summary>
        /// Закрывает окна и запрещает открывать новые.
        /// </summary>
        public void BlockAll() => _uiManager.BlockAll(this);

        /// <summary>
        /// Снимает блокировку, поставленную этим мостом.
        /// </summary>
        public void UnblockAll() => _uiManager.UnblockAll(this);

        /// <summary>
        /// Вызывать из сигнала OnCutsceneStart.
        /// </summary>
        public void BeginCutscene() => _uiManager.BeginCutscene();

        /// <summary>
        /// Вызывать из сигнала OnCutsceneEnd.
        /// </summary>
        public void EndCutscene() => _uiManager.EndCutscene();
    }
}
