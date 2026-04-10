using UnityEngine;

[CreateAssetMenu(menuName = "Items/Reactions/Debug Reaction")]
public class DebugReaction : ItemReactionNight
{
    [TextArea]
    public string message;

    public override void Execute(ItemTarget target)
    {
        Debug.Log($"[ItemReaction] {message} on {target.name}");
    }
}