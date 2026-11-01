using UnityEngine;
using Zenject;

namespace UI
{
    /// <summary>
    /// Единственное место, где читаются Esc и хоткеи окон. Окна сами клавиши не слушают,
    /// иначе они обходят блокировки UIManager и дважды реагируют на одно нажатие.
    /// </summary>
    public class UIInputHandler : ITickable
    {
        private const KeyCode CancelKey = KeyCode.Escape;

        private readonly UIManager _uiManager;

        public UIInputHandler(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public void Tick()
        {
            if (Input.GetKeyDown(CancelKey))
            {
                _uiManager.HandleCancel();
                return;
            }

            var ids = _uiManager.ScreenIds;
            for (int i = 0; i < ids.Count; i++)
            {
                KeyCode key = ids[i].ToggleKey;
                if (key != KeyCode.None && Input.GetKeyDown(key))
                {
                    _uiManager.Toggle(ids[i]);
                    return;
                }
            }
        }
    }
}
