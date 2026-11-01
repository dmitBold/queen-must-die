using UnityEngine;

namespace UI
{
    /// <summary>
    /// Идентификатор окна. ScriptableObject, потому что UnityEvent не передаёт enum, а строки хрупкие:
    /// ассет можно выбрать в инспекторе у UIBridge и у самого окна.
    /// </summary>
    [CreateAssetMenu(fileName = "UIScreenId", menuName = "UI/Screen Id")]
    public class UIScreenId : ScriptableObject
    {
        [Tooltip("Клавиша открытия и закрытия окна. None, если окно открывается только кодом или по Esc.")]
        [SerializeField] private KeyCode toggleKey = KeyCode.None;

        [Tooltip("Окно паузы (настройки). Его открывает Esc, и пока оно открыто, другие окна открыть нельзя.")]
        [SerializeField] private bool isPauseMenu;

        /// <summary>
        /// Клавиша открытия и закрытия окна.
        /// </summary>
        public KeyCode ToggleKey => toggleKey;

        /// <summary>
        /// Истина для окна паузы.
        /// </summary>
        public bool IsPauseMenu => isPauseMenu;
    }
}
