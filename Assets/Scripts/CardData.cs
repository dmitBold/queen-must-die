using UnityEngine;
using System.Collections.Generic;


public enum CardEffectType
{
    ModifyStat,
    LockStat
}

[System.Serializable]
public class Effect
{
    public CardEffectType type;
    public WorldState.Stats stat;
    public int value;
}

[System.Serializable]
public class Choice
{
    public string text;
    public List<Effect> effects;
    public List<string> addFlags;
    public List<string> removeFlags;

    [TextArea]
    public string reactionText;

    //test
    public ItemData rewardItem;
    public int rewardAmount = 1;
}

//testtesttest
[System.Serializable]
public class ItemReaction
{
    public ItemData item;

    public List<Effect> effects;
    public List<string> addFlags;
    public List<string> removeFlags;

    [TextArea]
    public string reactionText;
}




[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public string CardText;
    public Choice LeftChoice;
    public Choice RightChoice;

    public List<string> RequiredFlags;
    public List<string> BlockedFlags;
    public int MinWeek;
    public int MaxWeek;
    public int weight;

    public int cooldown;

    public VisitorData visitor;

    //test
    public List<ItemReaction> itemReactions;

    //test

    //test
    public bool IsMandatory;
    public bool IsRandom;
    //test

    public ItemReaction GetReactionForItem(ItemData item)
    {
        foreach (var reaction in itemReactions)
        {
            if (reaction.item == item)
                return reaction;
        }
        return null;
    }
}
