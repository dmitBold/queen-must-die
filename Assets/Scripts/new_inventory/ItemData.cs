using UnityEngine;

namespace Inventory
{
    public enum ItemType
    {
        Normal,
        StaticBase,   // Не трогаем, для ночных уровней
        PortableBase, // Каркас, который можно крутить в инвентаре
        Part          // Деталь для сборки
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Base Info")]
        public string id;
        public string itemName;

        [TextArea]
        public string description;

        [Header("Visual")]
        public Sprite icon;

        [Header("New Mechanics")]
        public ItemType itemType;

        [Tooltip("Префаб с AssemblyView для спавна (только для PortableBase)")]
        public GameObject assemblyPrefab;

        [Tooltip("Что мы получим после полной сборки каркаса")]
        public ItemData resultItem;

        public bool CanApplyDay = true;
        public bool CanApplyNight = true;
    }
}