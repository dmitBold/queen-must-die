using UI;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Показывает ночной диалог, когда игрок закроет все окна. Вешается на вью сборки и вызывается из её событий.
    /// </summary>
    public class PostWindowDialogue : MonoBehaviour
    {
        /// <summary>Страницы диалога, по одной реплике на клик.</summary>
        [SerializeField, TextArea(3, 5)] private string[] pages;

        /// <summary>Имена говорящих для соответствующих страниц.</summary>
        [SerializeField] private string[] names;

        private UIManager _uiManager;

        [Inject]
        public void Construct(UIManager uiManager)
        {
            _uiManager = uiManager;
        }

        /// <summary>
        /// Ставит диалог в очередь до закрытия окон. Днём ночного менеджера диалогов нет, и диалог не показывается.
        /// </summary>
        public void Play()
        {
            _uiManager.RunWhenInteractive(Show);
        }

        private void Show()
        {
            NightDialogueManager manager = NightDialogueManager.Instance;
            if (manager == null)
                return;

            // Режим Focused запрещает ходить, пока игрок читает реплику.
            _uiManager.BeginFocus(() =>
            {
                manager.ForceEndDialogue();
                _uiManager.EndFocus();
            });
            manager.StartDialogue(null, pages, names, _uiManager.EndFocus);
        }
    }
}
