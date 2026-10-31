using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ObjectActivationTimer : MonoBehaviour
{
    [System.Serializable]
    public class ActivationGroup
    {
        public string groupName;
        public float delay;
        public List<GameObject> ObjectsToActivate;
        public List<GameObject> ObjectsToDeactivate;
        public UnityEvent customEvent;
    }

    [SerializeField] private List<ActivationGroup> activationGroups;
    [SerializeField] private bool PlayOnStart;
    [SerializeField] private bool LoopSequence;

    void Start()
    {
        if (PlayOnStart)
        {
            StartSequence();
        }
    }

    public void StartSequence()
    {
        StartCoroutine(ExecuteSequence());
    }

    IEnumerator ExecuteSequence()
    {
        foreach (var group in activationGroups)
        {
            
            foreach (var obj in group.ObjectsToActivate)
            {
                //obj.gameObject.SetActive(true);
                obj.SetActive(true);
                //yield return new WaitForSeconds(group.delay);
            }

            foreach (var obj in group.ObjectsToDeactivate)
            {
                obj.SetActive(false);
                //yield return new WaitForSeconds(group.delay);
            }
            group.customEvent?.Invoke();
            yield return new WaitForSeconds(group.delay);
        }
    }

}
