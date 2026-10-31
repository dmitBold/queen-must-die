using UnityEngine;
using UnityEngine.Events;

public class ContInteractGroup : MonoBehaviour
{
    public string Code;
    public string currentString = "";

    public UnityEvent SolveEvent;

    public void Solve()
    {
        SolveEvent?.Invoke();
    }

}
