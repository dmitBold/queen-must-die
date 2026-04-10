using System.Collections.Generic;
using UnityEngine;

public enum DaySlotType
{
    Mandatory,
    Random
}

[System.Serializable]
public class DaySlot
{
    public DaySlotType type;
}

[CreateAssetMenu(menuName = "Day/Day Plan")]
public class DayPlan : ScriptableObject
{
    public List<DaySlot> slots;
}