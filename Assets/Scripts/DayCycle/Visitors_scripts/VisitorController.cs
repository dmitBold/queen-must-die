using System;
using Cards;
using Environment;
using UnityEngine;
using Zenject;
using Core;
using FMODUnity;

namespace Visitors
{
    public class VisitorController : MonoBehaviour
    {
        public event Action OnVisitorArrived;
        public event Action OnVisitorLeft;

        [SerializeField] private CageController cage;
        [SerializeField] private Transform visitorSlot;

        private GameObject currentVisitor;
        public CardData currentCard;

        public EventReference cage_arrive;
        public EventReference cage_leave;

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        void Awake()
        {
            cage.OnArrived += Arrived;
            cage.OnLeft += Left;
        }

        public void Spawn(CardData card)
        {
            Debug.Log("Visitor: coming");
            currentCard = card;
            if (currentCard.visitor != null)
            {
                var visitorPrefab = currentCard.visitor.prefab;
                currentVisitor = Instantiate(visitorPrefab, visitorSlot);

                currentVisitor.transform.localRotation = Quaternion.Euler(card.visitor.localRotation);
                currentVisitor.transform.localScale = Vector3.one * card.visitor.scaleMultiplier;
                currentVisitor.transform.localPosition = card.visitor.localOffset;   

            }

            _audioService.PlayFMODEvent(cage_arrive);

            cage.PlayArrival();
        }

        public void Despawn()
        {
            Debug.Log("Visitor: leaving");
            _audioService.PlayFMODEvent(cage_leave);
            cage.PlayLeave();
        }

        void Arrived()
        {
            Debug.Log("Visitor: arrived");
            if (currentCard == null)
            {
                Debug.LogError("Arrived but currentCard is null");
                return;
            }
            if (currentCard.visitor == null || currentCard.visitor.prefab == null)
            {
                Debug.LogWarning("Card has no visitor data, spawning empty cage");
                OnVisitorArrived?.Invoke();
                return;
            }


            _audioService.PlayFMODEvent(currentCard.visitor.arrivalSound);

            //var visitorPrefab = currentCard.visitor.prefab;
            //currentVisitor = Instantiate(visitorPrefab, visitorSlot);
            //currentVisitor.transform.localPosition = Vector3.zero;

            if (currentCard.visitor != null)
            {
                var anim = currentVisitor.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.runtimeAnimatorController = currentCard.visitor.animator;
                }
            }

            OnVisitorArrived?.Invoke();
        }

        void Left()
        {
            Debug.Log("Visitor: left");
            if (currentVisitor != null)
            {
                Destroy(currentVisitor);
            }

            currentVisitor = null;
            currentCard = null;

            OnVisitorLeft?.Invoke();
        }

        /*public void OnCageArrived()
    {
        Arrived();
    }

    public void OnCageLeft()
    {
        Left();
    }*/

    }
}
