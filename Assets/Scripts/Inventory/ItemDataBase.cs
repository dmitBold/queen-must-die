using Inventory;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> allItems;
    public ItemData GetItemById(string id)
    {
        foreach (var item in allItems)
        {
            if (item != null && item.id == id)
                return item;
        }
        Debug.LogError($"[ItemDatabase] Предмет с ID {id} не найден!");
        return null;
    }
}
