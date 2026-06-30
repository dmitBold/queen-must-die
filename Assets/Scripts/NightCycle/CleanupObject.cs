using Core;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class CleanupObject : MonoBehaviour
    {
        [Header("Settings")]
        //[SerializeField] private GameObject cleanupEffect; // ������ ������
        [SerializeField] private AudioClip cleanupSound;   // ���� ������
        [SerializeField] private GameObject cleanObject;

        [SerializeField] CleanupStage parentStage;

        [SerializeField] private GameObject clear_GROUP;
        //new
        [SerializeField] private Animator anim;
        [SerializeField] private string trig;
        [SerializeField] private Interactable this_inter;
        [SerializeField] private List<Interactable> other_inter;

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void Clean()
        {
            if (cleanupSound != null && _audioService != null)
            {
                _audioService.PlaySound(cleanupSound);
            }

            //
            //if (cleanupEffect != null)
            //{
            //  Instantiate(cleanupEffect, transform.position, Quaternion.identity);
            //}

            if (parentStage != null)
            {
                parentStage.OnObjectCleaned();
            }

            gameObject.SetActive(false);
            if(clear_GROUP != null){
                clear_GROUP.SetActive(false);
            }
            cleanObject.SetActive(true);
        }

        public void Clean_with_anim()
        {
            if (cleanupSound != null && _audioService != null)
            {
                _audioService.PlaySound(cleanupSound);
            }

            if (this_inter != null) { this_inter.enabled = false; }

            anim.SetTrigger(trig);

            if (other_inter.Count > 0)
            {
                foreach(Interactable interactable in other_inter)
                {
                    interactable.enabled = false;
                }
            }

            if (parentStage != null)
            {
                parentStage.OnObjectCleaned();
            }

        }

    }
}