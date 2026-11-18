using System.Collections.Generic;
using Core;
using FMODUnity;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class StatueController : MonoBehaviour {

        public List<GameObject> poses;
        public EventReference MoveSound;
        
        int curr_pose_index = 0;

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

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
                _audioService.PlayFMODEvent(MoveSound);
                poses[curr_pose_index].SetActive(false);
                curr_pose_index++;
                poses[curr_pose_index].SetActive(true);
            }
        }
    
    }
}
