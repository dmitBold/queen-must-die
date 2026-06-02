using System;
using System.Collections.Generic;

namespace Inventory
{
    public class InventoryManager
    {
        Dictionary<ItemData, int> items = new();

        public event Action OnInventoryChanged;

        public IReadOnlyDictionary<ItemData, int> Items => items;

        public bool HasItem(ItemData item)
        {
            return items.ContainsKey(item);
        }

        public int GetCount(ItemData item)
        {
            return items.TryGetValue(item, out int count) ? count : 0;
        }

        public void AddItem(ItemData item, int amount = 1)
        {
            //Debug.Log($"[AddItem] InventoryManager ID = {this.GetInstanceID()}");
            if (item == null || amount <= 0)
                return;

            if (items.ContainsKey(item))
                items[item] += amount;
            else
                items[item] = amount;

            OnInventoryChanged?.Invoke();
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (!items.ContainsKey(item) || amount <= 0)
                return false;

            items[item] -= amount;

            if (items[item] <= 0)
                items.Remove(item);

            OnInventoryChanged?.Invoke();
            return true;
        }

        //
        public List<ItemData> GetAllItems()
        {
            List<ItemData> result = new();

            foreach (var pair in items)
            {
                for (int i = 0; i < pair.Value; i++)
                    result.Add(pair.Key);
            }

            return result;
        }
    }
}