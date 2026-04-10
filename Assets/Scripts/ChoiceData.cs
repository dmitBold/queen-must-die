using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChoiceData", menuName = "Scriptable Objects/ChoiceData")]
public class ChoiceData : ScriptableObject
{
    public string ChoiceText;
    public List<EffectData> effects = new List<EffectData>();
}