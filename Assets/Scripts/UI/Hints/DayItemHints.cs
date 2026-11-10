using Core;
using Inventory;
using UnityEngine;
using Zenject;

/// <summary>
/// Подсказки дня про сборку меча:
/// 1) получена гарда (Tang) -> «нажмите I»;
/// 2) после этого игрок впервые открыл инвентарь -> «Бет может собирать малые предметы»;
/// 3) получен клинок (Blade) -> «соберите меч».
/// Каждая подсказка показывается один раз за игру (флаги в WorldState, сохраняются вместе с игрой).
/// </summary>
public class DayItemHints : MonoBehaviour
{
    [Header("Предметы")]
    public ItemData tang;
    public ItemData blade;

    [Header("Тексты")]
    [TextArea] public string tangHint = "Получен предмет: Гарда. Нажмите I, чтобы открыть инвентарь.";
    [TextArea] public string inventoryHint = "\u0411\u0435\u0442 \u043C\u043E\u0436\u0435\u0442 \u0441\u043E\u0431\u0438\u0440\u0430\u0442\u044C \u043F\u0440\u0435\u0434\u043C\u0435\u0442\u044B \u0438\u0437 \u0434\u0435\u0442\u0430\u043B\u0435\u0439 \u0432 \u0438\u043D\u0432\u0435\u043D\u0442\u0430\u0440\u0435, \u043B\u0435\u0436\u0430\u0449\u0438\u0435 \u0432 \u0438\u043D\u0432\u0435\u043D\u0442\u0430\u0440\u0435";
    [TextArea] public string bladeHint = "Соберите меч, открыв инвентарь";

    [Header("Показ")]
    public float displayDuration = 4f;
    public float fadeDuration = 0.5f;

    const string TangFlag = "HINT_TANG", InventoryFlag = "HINT_INVENTORY", BladeFlag = "HINT_BLADE";

    [Inject] private HintManager _hints;
    [Inject] private InventoryManager _inventory;
    [InjectOptional] private WorldState _worldInjected;
    private WorldState _world;
    private InventoryUI[] _inventoryUIs;

    private void Start()
    {
        _world = _worldInjected != null ? _worldInjected : FindFirstObjectByType<WorldState>();
        _inventoryUIs = FindObjectsByType<InventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (_inventory != null) _inventory.OnInventoryChanged += CheckItems;
        CheckItems();
    }

    private void OnDestroy()
    {
        if (_inventory != null) _inventory.OnInventoryChanged -= CheckItems;
    }

    private bool Has(string flag) => _world != null && _world.flags.Contains(flag);
    private void Mark(string flag) { if (_world != null) _world.AddFlag(flag); }

    private void Show(string text)
    {
        if (_hints != null && !string.IsNullOrEmpty(text)) _hints.ShowHint(text, displayDuration, fadeDuration);
    }

    private void CheckItems()
    {
        if (_inventory == null) return;
        if (tang != null && !Has(TangFlag) && _inventory.HasItem(tang)) { Mark(TangFlag); Show(tangHint); }
        if (blade != null && !Has(BladeFlag) && _inventory.HasItem(blade)) { Mark(BladeFlag); Show(bladeHint); }
    }

    private void Update()
    {
        if (!Has(TangFlag) || Has(InventoryFlag)) return;
        if (_inventoryUIs == null) return;
        foreach (var ui in _inventoryUIs)
            if (ui != null && ui.isOpen) { Mark(InventoryFlag); Show(inventoryHint); break; }
    }
}
