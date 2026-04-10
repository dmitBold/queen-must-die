using UnityEngine;

public abstract class ItemReactionNight : ScriptableObject
{
    public abstract void Execute(ItemTarget target);
}

/*[CreateAssetMenu(menuName = "Items/Reactions/Debug Reaction")]
public class DebugReaction : ItemReactionNight
{
    [TextArea]
    public string message;

    public override void Execute(ItemTarget target)
    {
        Debug.Log($"[ItemReaction] {message} on {target.name}");
    }
}*/

/*[System.Serializable]
public class ItemReactionEntry
{
    public ItemData item;
    public ItemReactionNight reaction;
}*/