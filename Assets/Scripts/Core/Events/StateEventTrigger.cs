using Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class StateEventTrigger : MonoBehaviour
{
    private WorldState worldState;
    public bool triggerOnStart;

    [Tooltip("События соответствующие каждому флагу в WorldState")]
    public List<WorldFlagEvent> stageEvents = new List<WorldFlagEvent>();

    [System.Serializable]
    public class WorldFlagEvent
    {
        public string flag;
        public UnityEvent Event;
    }

    [Inject]
    public void Constructor(WorldState state)
    {
        this.worldState = state;
    }

    public void TriggerEvents()
    {
        foreach (WorldFlagEvent FlagEvent in stageEvents)
        {
            if (worldState.HasFlag(FlagEvent.flag))
            {
                FlagEvent.Event?.Invoke();
            }
        }
    }


    private void Start()
    {
        if (triggerOnStart)
        {
            TriggerEvents();
        }
    }

}
