using Core;
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

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void Clean()
        {
            if (cleanupSound != null)
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
    }
}