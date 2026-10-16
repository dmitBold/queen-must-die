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

        // ��������� ������ �� ������, ���� ���-�� ��� ��� �����, �� ��� ������ ������ ����� 3D
        [Header("Visual")]
        public Sprite icon;

        [Header("3D UI Preview (�����)")]
        [Tooltip("������, ������� ����� ��������� � ���������")]
        public GameObject uiModelPrefab;
        [Tooltip("��������� �������� ������ ���������� ��� ����� ���������")]
        public Vector3 uiModelScale = Vector3.one;

        [Header("New Mechanics")]
        public ItemType itemType;

        [Tooltip("������ � AssemblyView ��� ������ (������ ��� PortableBase)")]
        public GameObject assemblyPrefab;

        [Tooltip("��� �� ������� ����� ������ ������ �������")]
        public ItemData resultItem;

        public bool CanApplyDay = true;
        public bool CanApplyNight = true;
    }
}