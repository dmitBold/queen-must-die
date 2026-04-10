using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData_alternative")]
public class CardData_alternative : ScriptableObject
{
    public string CardText;
    public ChoiceData leftchoice;
    public ChoiceData rightchoice;
}