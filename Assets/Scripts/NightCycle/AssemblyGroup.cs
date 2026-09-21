using NightCycle;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AssemblyGroup : MonoBehaviour
{
    public List<AssemblyInteractable> assemblies;
    public UnityEvent OnCompleted;

    public void CheckCompletion()
    {
        foreach (AssemblyInteractable assembly in assemblies)
        {
            if (!assembly.isCompleted)
            {
                return;
            }
        }
        OnCompleted?.Invoke();
    }

}