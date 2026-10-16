using System.Collections.Generic;
using Inventory;
using UnityEngine;

namespace NightCycle
{
    public class ItemTarget : MonoBehaviour, IFocusable
    {
        [Header("Allowed Items")]
        [SerializeField] List<ItemReactionEntry> reactions = new();
        [SerializeField] InventoryUI inventoryUI;
        Dictionary<ItemData, ItemReactionNight> lookup;

        public void OnEnterFocus()
        {
            // ������ ��������� ����������� PlayerMode, �� ������ ������� UI: "�������� ��� ����"
            //inventoryUI.OpenForItemTarget(this);
        }

        public void OnExitFocus()
        {
            //inventoryUI.ExitItemSelection();
        }

        void Awake()
        {
            lookup = new Dictionary<ItemData, ItemReactionNight>();

            foreach (var entry in reactions)
            {
                if (entry.item != null && entry.reaction != null)
                    lookup[entry.item] = entry.reaction;
            }
        }

        public bool CanApply(ItemData item)
        {
            return item != null && lookup.ContainsKey(item);
        }

        public void Apply(ItemData item)
        {
            if (!CanApply(item))
                return;

            lookup[item].Execute(this);
        }
    }
}
