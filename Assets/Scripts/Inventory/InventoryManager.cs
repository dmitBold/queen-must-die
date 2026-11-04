using System;
using System.Collections.Generic;
using Zenject;
using static SaveManager;

namespace Inventory
{
    public class InventoryManager
    {
        //test
        [Inject] private ItemDatabase _itemDatabase;
        //test

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

        /// <summary>
        /// Атомарно заменяет один экземпляр oldItem на newItem с единственным OnInventoryChanged.
        /// Если oldItem был единственным, newItem встаёт на его позицию в порядке инвентаря
        /// (если newItem уже есть в инвентаре, счётчик просто увеличивается). Иначе newItem добавляется в конец.
        /// Возвращает false, если oldItem отсутствует или предметы совпадают.
        /// </summary>
        public bool ReplaceItem(ItemData oldItem, ItemData newItem)
        {
            if (oldItem == null || newItem == null || oldItem == newItem)
                return false;

            if (!items.TryGetValue(oldItem, out int oldCount) || oldCount <= 0)
                return false;

            if (oldCount == 1 && !items.ContainsKey(newItem))
            {
                // Порядок словаря не гарантируется после удалений, поэтому пересобираем его явно:
                // UI опирается на позицию предмета, чтобы обновить слот на месте.
                var snapshot = new List<KeyValuePair<ItemData, int>>(items);
                items.Clear();

                foreach (var pair in snapshot)
                {
                    if (pair.Key == oldItem)
                        items[newItem] = 1;
                    else
                        items[pair.Key] = pair.Value;
                }
            }
            else
            {
                if (oldCount == 1)
                    items.Remove(oldItem);
                else
                    items[oldItem] = oldCount - 1;

                items[newItem] = items.TryGetValue(newItem, out int newCount) ? newCount + 1 : 1;
            }

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

        //test
        public List<SavedItem> GetInventorySaveData()
        {
            List<SavedItem> savedList = new List<SavedItem>();

            foreach (var pair in items)
            {
                if (pair.Key != null && !string.IsNullOrEmpty(pair.Key.id))
                {
                    savedList.Add(new SavedItem(pair.Key.id, pair.Value));
                }
            }

            return savedList;
        }

        public void LoadInventoryData(List<SavedItem> savedInventory)
        {
            items.Clear();

            if (savedInventory == null) return;

            foreach (var savedItem in savedInventory)
            {
                ItemData originalItem = _itemDatabase.GetItemById(savedItem.itemId);

                if (originalItem != null)
                {
                    AddItem(originalItem, savedItem.amount);
                }
            }

            OnInventoryChanged?.Invoke();
        }
        //test
    }
}