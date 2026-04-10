using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class StatueController : MonoBehaviour {

    public List<GameObject> poses;
    public AudioClip MoveSound;

    int curr_pose_index = 0;

    private void Start()
    {
        foreach (GameObject pose in poses) {
            pose.SetActive(false);
        }
        poses[0].SetActive(true);

    }

    public void advance_pose()
    {
        if (curr_pose_index < poses.Count - 1)
        {
            SoundManager.Instance.PlaySound(MoveSound);
            poses[curr_pose_index].SetActive(false);
            curr_pose_index++;
            poses[curr_pose_index].SetActive(true);
        }
    }
    
}
