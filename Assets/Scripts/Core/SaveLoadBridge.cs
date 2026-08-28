using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class SaveLoadBridge : MonoBehaviour
    {
        private SaveManager _saveManager;

        [Inject]
        public void Construct(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public void TriggerSave()
        {
            _saveManager.SaveGame();
            Debug.Log("[SaveLoadBridge] Вызвано сохранение игры.");
        }

        public void TriggerLoad()
        {
            bool success = _saveManager.LoadGame();
            if (success)
            {
                Debug.Log("[SaveLoadBridge] Вызвана загрузка последнего сейва.");
            }
            else
            {
                Debug.Log("[SaveLoadBridge] Ошибка загрузки (нет файла).");
            }
        }
    }
}