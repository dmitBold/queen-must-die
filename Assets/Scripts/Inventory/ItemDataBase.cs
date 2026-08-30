using Inventory;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> allItems;

    // Кэшированный словарь для моментального поиска
    private Dictionary<string, ItemData> _itemDict;

    private void Initialize()
    {
        _itemDict = new Dictionary<string, ItemData>();
        foreach (var item in allItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.id))
            {
                _itemDict[item.id] = item;
            }
        }
    }

    public ItemData GetItemById(string id)
    {
        // Инициализируем словарь при первом обращении
        if (_itemDict == null || _itemDict.Count == 0)
        {
            Initialize();
        }

        if (_itemDict.TryGetValue(id, out var item))
        {
            return item;
        }

        Debug.LogError($"[ItemDatabase] Предмет с ID {id} не найден!");
        return null;
    }
}