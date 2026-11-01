using System;
using Inventory;
using UnityEngine;

namespace Cards
{
    /// <summary>
    /// Награда за прочтение страницы карты. Индекс элемента в CardData.pageRewards равен индексу страницы в CardPages.
    /// </summary>
    [Serializable]
    public class CardPageReward
    {
        [SerializeField] private ItemData item;
        [SerializeField] private int amount = 1;
        [SerializeField] private Quest quest;
        [SerializeField] private DayNoteData note;

        [Tooltip("Если включено, факт выдачи сохраняется (флаг в WorldState) и награда выдаётся один раз за всю игру. " +
                 "Если выключено, награда выдаётся при каждом показе карты (но не чаще одного раза за показ).")]
        [SerializeField] private bool saveGrantStatus = true;

        public ItemData Item => item;
        public int Amount => Mathf.Max(1, amount);
        public Quest Quest => quest;
        public DayNoteData Note => note;
        public bool SaveGrantStatus => saveGrantStatus;

        public bool IsEmpty => item == null && quest == null && note == null;
    }
}
