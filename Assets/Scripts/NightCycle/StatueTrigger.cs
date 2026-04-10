using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class StatueTrigger : MonoBehaviour
{
    public List<StatueController> controllers;
    public GameObject nextTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var controller in controllers)
            {
                controller.advance_pose();
            }

            if(nextTrigger != null)
            {
                nextTrigger.SetActive(true);
            }

            gameObject.SetActive(false);
        }
    }

}