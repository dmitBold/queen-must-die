using UnityEngine;

public class ContInteractable : MonoBehaviour
{
    public string part;
    public ContInteractGroup group;

    public void addPart()
    {
        group.currentString += part;

        CheckString();
    }

    public void CheckString()
    {
        if (!group.Code.StartsWith(group.currentString))
        {
            group.currentString = "";
        }

        if(group.currentString == group.Code)
        {
            group.Solve();
        }

    }

}
