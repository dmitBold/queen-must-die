using Cards;
using Inventory;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PageChoice
{
    public string text;
}



[CreateAssetMenu(fileName = "PageData", menuName = "Scriptable Objects/PageData")]
public class PageData : ScriptableObject
{
    public PageChoice LeftChoice;
    public PageChoice RightChoice;
    [TextArea(3, 5)]
    public string[] dialoguePages;
}
