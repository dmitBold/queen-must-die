using System;
using System.Collections.Generic;
using Core;
using DI;
using NightCycle;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace UI
{
    /// <summary>
    /// Центральный менеджер UI в ProjectContext. Держит единственное открытое окно, контексты
    /// (катсцена, сборка, фокус-объект, драг двери), блокировки и скрытие оверлеев,
    /// и единственный вызывает PlayerStateController.SetMode.
    /// Режим игрока вычисляется заново при каждом изменении состояния, поэтому закрытие одного окна
    /// не перебивает режим, который требует другой активный контекст.
    /// </summary>
    public class UIManager : IInitializable, ITickable, IDisposable
    {
        // Режим игрока в катсцене. Чтобы вернуть курсор и свободную мышь, достаточно заменить на PlayerMode.Focused.
        private const PlayerMode CutsceneMode = PlayerMode.Frozen;

        private readonly IPlayerProvider _playerProvider;
        private readonly SceneTransitionService _transitionService;

        private readonly Dictionary<UIScreenId, IUIScreen> _screens = new();
        private readonly List<UIScreenId> _screenIds = new();
        private readonly List<IUIHideable> _hideables = new();

        // Владельцы блокировок вместо булевых флагов: загрузка, катсцена и сцена могут блокировать одновременно,
        // и одна не должна снимать блок другой.
        private readonly HashSet<object> _blockOwners = new();
        private readonly HashSet<object> _hideOwners = new();

        private IUIScreen _current;
        private UIScreenId _currentId;

        private bool _cutsceneActive;
        private bool _doorState;

        private bool _assemblyActive;
        private Action _assemblyCancel;

        private bool _focusActive;
        private Action _focusCancel;

        private bool _hidden;
        private bool _wasLoading;
        private bool _isLoadingSceneActive;

        public UIManager(IPlayerProvider playerProvider, SceneTransitionService transitionService)
        {
            _playerProvider = playerProvider;
            _transitionService = transitionService;
        }

        /// <summary>
        /// Идентификаторы зарегистрированных окон. Читается UIInputHandler каждый кадр, поэтому список, а не перечисление.
        /// </summary>
        public IReadOnlyList<UIScreenId> ScreenIds => _screenIds;

        /// <summary>
        /// Истина, пока идёт переход через сцену загрузки или она активна. Окна и ввод в это время игнорируются.
        /// </summary>
        public bool IsLoading => _transitionService.IsTransitioning || _isLoadingSceneActive;

        /// <summary>
        /// Истина, если взаимодействие с миром разрешено: нет загрузки, катсцены, открытого окна, сборки и фокус-объекта.
        /// Выход из активного фокус-объекта этим флагом не блокируется: его обрабатывает PlayerInteraction сам.
        /// </summary>
        public bool CanInteract => !IsLoading && !_cutsceneActive && _current == null && !_assemblyActive && !_focusActive;

        private bool IsInputBlocked => IsLoading || _cutsceneActive || _blockOwners.Count > 0;

        // Сборка окна не блокирует: окна работают как вкладки, и открытие нового окна прерывает сборку.
        private bool CanOpenScreens => !IsInputBlocked && !_focusActive;

        // Счётчик вложенных составных операций: пока он больше нуля, режим игрока не пересчитывается,
        // чтобы смена окна и завершение сборки не давали промежуточный FreeMovement.
        private int _recomputeSuspend;

        public void Initialize()
        {
            _isLoadingSceneActive = SceneManager.GetActiveScene().name == SceneNames.Loading;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        public void Tick()
        {
            bool loading = IsLoading;
            if (loading && !_wasLoading)
            {
                // Сборку отменяем до выгрузки сцены, пока AssemblyService ещё может корректно выйти из сессии.
                CancelAssembly();
                CloseCurrent();
            }

            _wasLoading = loading;
            ApplyHidden();
        }

        /// <summary>
        /// Регистрирует окно. Вызывается самим окном при инициализации.
        /// </summary>
        public void Register(IUIScreen screen)
        {
            // Контекст нужен, чтобы клик по ошибке в консоли подсвечивал лишний объект в иерархии.
            UnityEngine.Object context = screen as UnityEngine.Object;

            if (screen == null || screen.Id == null)
            {
                Debug.LogError("[UIManager] Окно без UIScreenId не зарегистрировано.", context);
                return;
            }

            if (_screens.TryGetValue(screen.Id, out IUIScreen existing))
            {
                // Повторная регистрация того же экземпляра (двойной вызов Construct) не ошибка.
                if (ReferenceEquals(existing, screen))
                    return;

                Debug.LogError($"[UIManager] Окно '{screen.Id.name}' уже зарегистрировано другим объектом: " +
                               $"'{(existing as UnityEngine.Object)?.name}'. Лишний экземпляр в сцене или префабе нужно удалить.", context);
                return;
            }

            _screens.Add(screen.Id, screen);
            _screenIds.Add(screen.Id);
        }

        /// <summary>
        /// Регистрирует оверлей, который прячется через HideAll.
        /// </summary>
        public void RegisterHideable(IUIHideable hideable)
        {
            if (hideable == null || _hideables.Contains(hideable))
                return;

            _hideables.Add(hideable);
            if (_hidden)
                hideable.SetHidden(true);
        }

        /// <summary>
        /// Истина, если окно с этим идентификатором сейчас открыто.
        /// </summary>
        public bool IsOpen(UIScreenId id) => id != null && _currentId == id;

        /// <summary>
        /// Открывает окно, атомарно закрывая предыдущее (один пересчёт режима, без промежуточного Free).
        /// Активная сборка принудительно завершается: окна ведут себя как вкладки.
        /// Возвращает false, если открытие запрещено: загрузка, блокировка, катсцена, фокус-объект
        /// или открыто окно паузы.
        /// </summary>
        public bool Open(UIScreenId id)
        {
            if (id == null || !_screens.TryGetValue(id, out IUIScreen screen))
            {
                // Тихий отказ скрывал бы кнопки, привязанные к сценной копии окна без UIScreenId.
                Debug.LogWarning($"[UIManager] Открытие окна '{(id != null ? id.name : "null")}' невозможно: окно не зарегистрировано.");
                return false;
            }

            if (!CanOpenScreens)
                return false;

            if (_currentId == id)
                return true;

            if (_currentId != null && _currentId.IsPauseMenu)
                return false;

            _recomputeSuspend++;
            try
            {
                // Сначала окно: InventoryUI.Close завершает сборку PortableBase сам.
                // Потом остаток сборки, у которой окна нет (AssemblyInteractable в мире).
                _current?.Close();
                CancelAssembly();

                _current = screen;
                _currentId = id;
                screen.Open();
            }
            finally
            {
                _recomputeSuspend--;
            }

            Recompute();
            return true;
        }

        /// <summary>
        /// Закрывает окно, если оно сейчас открыто.
        /// </summary>
        public void Close(UIScreenId id)
        {
            if (IsOpen(id))
                CloseCurrent();
        }

        /// <summary>
        /// Закрывает текущее окно, если оно есть.
        /// </summary>
        public void CloseCurrent()
        {
            if (CloseCurrentInternal())
                Recompute();
        }

        /// <summary>
        /// Закрывает окно, если оно открыто (вместе с его сборкой), иначе открывает, прерывая активную сборку.
        /// </summary>
        public void Toggle(UIScreenId id)
        {
            if (IsOpen(id))
                CloseCurrent();
            else
                Open(id);
        }

        /// <summary>
        /// Обрабатывает Esc. Если есть сборка или окно, закрывает их целиком за одно нажатие (игрок сразу в Free),
        /// иначе выходит из фокус-объекта, иначе открывает окно паузы. Один Esc выполняет только одно действие.
        /// </summary>
        public void HandleCancel()
        {
            if (IsInputBlocked)
                return;

            // Сборка PortableBase идёт под открытым инвентарём: закрываем и окно, и сборку без промежуточного
            // возврата в инвентарь. Сборка из мира окна не имеет, её завершает CancelAssembly.
            if (_assemblyActive)
            {
                _recomputeSuspend++;
                try
                {
                    CloseCurrentInternal();
                    CancelAssembly();
                }
                finally
                {
                    _recomputeSuspend--;
                }

                Recompute();
                return;
            }

            if (_current != null)
            {
                CloseCurrent();
                return;
            }

            if (_focusActive)
            {
                CancelFocus();
                return;
            }

            OpenPauseMenu();
        }

        /// <summary>
        /// Закрывает все окна и запрещает открывать новые, пока не снята блокировка всеми владельцами.
        /// </summary>
        public void BlockAll(object owner)
        {
            if (owner == null)
                return;

            _blockOwners.Add(owner);
            CloseCurrent();
        }

        /// <summary>
        /// Снимает блокировку владельца. Окна открываются снова, когда не осталось ни одного владельца.
        /// </summary>
        public void UnblockAll(object owner)
        {
            if (owner != null)
                _blockOwners.Remove(owner);
        }

        /// <summary>
        /// Прячет все зарегистрированные оверлеи. Логику окон не затрагивает.
        /// </summary>
        public void HideAll(object owner)
        {
            if (owner == null)
                return;

            _hideOwners.Add(owner);
            ApplyHidden();
        }

        /// <summary>
        /// Снимает скрытие владельца. Оверлеи появляются, когда не осталось ни одного владельца.
        /// </summary>
        public void ShowAll(object owner)
        {
            if (owner == null)
                return;

            _hideOwners.Remove(owner);
            ApplyHidden();
        }

        /// <summary>
        /// Начало катсцены: принудительно закрывает окна, отменяет сборку и фокус-объект, замораживает игрока.
        /// Повторный вызов без EndCutscene игнорируется, потому что сигналы таймлайна могут прийти дважды.
        /// </summary>
        public void BeginCutscene()
        {
            if (_cutsceneActive)
                return;

            _cutsceneActive = true;
            CancelAssembly();
            CancelFocus();
            CloseCurrentInternal();
            Recompute();
        }

        /// <summary>
        /// Конец катсцены. Безопасно вызывать без BeginCutscene.
        /// </summary>
        public void EndCutscene()
        {
            if (!_cutsceneActive)
                return;

            _cutsceneActive = false;
            Recompute();
        }

        /// <summary>
        /// Начало сборки: переводит игрока в Focused. Текущее окно остаётся открытым:
        /// для PortableBase инвентарь виден под сборкой. Esc, другое окно или Toggle закрывают и окно, и сборку.
        /// </summary>
        /// <param name="cancel">Вызывается при Esc, открытии другого окна, начале катсцены и загрузке. Должен завершить сессию и вызвать EndAssembly.</param>
        public void BeginAssembly(Action cancel)
        {
            if (_assemblyActive)
                return;

            _assemblyActive = true;
            _assemblyCancel = cancel;
            Recompute();
        }

        /// <summary>
        /// Конец сборки. Режим игрока пересчитывается: если окно осталось открытым, игрок остаётся в Focused.
        /// </summary>
        public void EndAssembly()
        {
            if (!_assemblyActive)
                return;

            ClearAssembly();
            Recompute();
        }

        /// <summary>
        /// Начало фокус-объекта (записка в мире, пазл). Окна закрываются, игрок переходит в Focused.
        /// </summary>
        /// <param name="cancel">Вызывается при Esc и начале катсцены. Должен выйти из фокуса и вызвать EndFocus.</param>
        public void BeginFocus(Action cancel)
        {
            _focusActive = true;
            _focusCancel = cancel;
            CloseCurrentInternal();
            Recompute();
        }

        /// <summary>
        /// Конец фокус-объекта.
        /// </summary>
        public void EndFocus()
        {
            if (!_focusActive)
                return;

            ClearFocus();
            Recompute();
        }

        /// <summary>
        /// Включает и выключает состояние драга двери (движение без поворота камеры).
        /// </summary>
        public void SetDoorState(bool active)
        {
            _doorState = active;
            Recompute();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Assembly грузится аддитивно и не должна сбрасывать состояние.
            if (mode == LoadSceneMode.Single)
                Reset();
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            _isLoadingSceneActive = next.name == SceneNames.Loading;
        }

        // Колбэки отмены здесь не вызываем: сцена уже сменилась, AssemblyController и фокус-объект могут быть уничтожены.
        // Сборку корректно завершает Tick в момент старта загрузки.
        private void Reset()
        {
            CloseCurrentInternal();
            _blockOwners.Clear();
            _hideOwners.Clear();
            _cutsceneActive = false;
            _doorState = false;
            ClearAssembly();
            ClearFocus();
            ApplyHidden();
            Recompute();
        }

        private bool CloseCurrentInternal()
        {
            if (_current == null)
                return false;

            IUIScreen screen = _current;
            _current = null;
            _currentId = null;
            screen.Close();
            return true;
        }

        private void OpenPauseMenu()
        {
            for (int i = 0; i < _screenIds.Count; i++)
            {
                if (_screenIds[i].IsPauseMenu)
                {
                    Open(_screenIds[i]);
                    return;
                }
            }

            Debug.LogWarning("[UIManager] Окно паузы не зарегистрировано: у UIScreenId нет флага IsPauseMenu.");
        }

        private void CancelAssembly()
        {
            if (!_assemblyActive)
                return;

            _assemblyCancel?.Invoke();

            // Если колбэк не вызвал EndAssembly, состояние не должно остаться висеть.
            if (_assemblyActive)
            {
                ClearAssembly();
                Recompute();
            }
        }

        private void CancelFocus()
        {
            if (!_focusActive)
                return;

            _focusCancel?.Invoke();

            if (_focusActive)
            {
                ClearFocus();
                Recompute();
            }
        }

        private void ClearAssembly()
        {
            _assemblyActive = false;
            _assemblyCancel = null;
        }

        private void ClearFocus()
        {
            _focusActive = false;
            _focusCancel = null;
        }

        private void ApplyHidden()
        {
            bool desired = IsLoading || _hideOwners.Count > 0;
            if (desired == _hidden)
                return;

            _hidden = desired;
            for (int i = 0; i < _hideables.Count; i++)
                _hideables[i].SetHidden(desired);
        }

        private PlayerMode ResolveMode()
        {
            if (_cutsceneActive)
                return CutsceneMode;

            if (_assemblyActive || _focusActive || _current != null)
                return PlayerMode.Focused;

            if (_doorState)
                return PlayerMode.DoorState;

            return PlayerMode.FreeMovement;
        }

        // Игрока нет в дневной сцене и в меню: тогда режим применять некуда, и это не ошибка.
        private void Recompute()
        {
            if (_recomputeSuspend > 0)
                return;

            PlayerView view = _playerProvider.PlayerView;
            if (view == null)
                return;

            PlayerStateController controller = view.PlayerStateController;
            if (controller == null)
                return;

            controller.SetMode(ResolveMode());
        }
    }
}
