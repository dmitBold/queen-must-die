using UnityEngine;

namespace Inventory
{
    public enum ItemType
    {
        Normal,
        StaticBase,
        PortableBase,
        Part
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Base Info")]
        public string id;
        public string itemName;

        [TextArea]
        public string description;

        // Оставляем иконку на случай, если где-то еще она нужна, но для слотов теперь юзаем 3D
        [Header("Visual")]
        public Sprite icon;

        [Header("3D UI Preview (Новое)")]
        [Tooltip("Префаб, который будет крутиться в инвентаре")]
        public GameObject uiModelPrefab;
        [Tooltip("Настройка масштаба модели специально для слота инвентаря")]
        public Vector3 uiModelScale = Vector3.one;

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