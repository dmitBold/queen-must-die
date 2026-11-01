using UnityEngine;
using Zenject;

namespace UI
{
    /// <summary>
    /// Вешается на корень оверлея (HUD, подсказки, фонарик), который должен пропадать по UIManager.HideAll.
    /// Прячет через CanvasGroup и не трогает активность объектов, поэтому собственные фейды и логика оверлея работают как раньше.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIHideableGroup : MonoBehaviour, IUIHideable
    {
        [Tooltip("CanvasGroup корня оверлея. Если пусто, берётся с этого объекта.")]
        [SerializeField] private CanvasGroup group;

        private float _shownAlpha;
        private bool _shownBlocksRaycasts;
        private bool _shownInteractable;
        private bool _hidden;

        [Inject]
        private void Construct(UIManager uiManager)
        {
            uiManager.RegisterHideable(this);
        }

        private void Awake()
        {
            if (group == null)
                group = GetComponent<CanvasGroup>();
        }

        public void SetHidden(bool hidden)
        {
            if (hidden == _hidden)
                return;

            _hidden = hidden;

            if (hidden)
            {
                // Запоминаем значения: корневой CanvasGroup мог быть настроен под свой фейд или блокировку кликов.
                _shownAlpha = group.alpha;
                _shownBlocksRaycasts = group.blocksRaycasts;
                _shownInteractable = group.interactable;
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }
            else
            {
                group.alpha = _shownAlpha;
                group.blocksRaycasts = _shownBlocksRaycasts;
                group.interactable = _shownInteractable;
            }
        }
    }
}
