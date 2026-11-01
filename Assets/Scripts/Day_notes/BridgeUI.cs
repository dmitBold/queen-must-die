using UI;
using UnityEngine;
using Zenject;

/// <summary>
/// Мост для UnityEvent в сценах: блокирует и разблокирует окна через UIManager.
/// Имя сохранено, чтобы не перенастраивать события в сценах.
/// </summary>
public class BridgeUI : MonoBehaviour
{
    private UIManager _uiManager;

    [Inject]
    public void Constructor(UIManager uiManager)
    {
        _uiManager = uiManager;
    }

    public void LockUI()
    {
        _uiManager.BlockAll(this);
    }

    public void UnlockUI()
    {
        _uiManager.UnblockAll(this);
    }
}
