using UnityEngine;
using System;
public class VisitorController : MonoBehaviour
{

    public event Action OnVisitorArrived;
    public event Action OnVisitorLeft;

    //[SerializeField] float arriveDelay = 1.0f;
    //[SerializeField] float leaveDelay = 1.0f;

    [SerializeField] CageController cage;
    [SerializeField] Transform visitorSlot;

    GameObject currentVisitor;
    public CardData currentCard;

    public AudioClip cage_arrive;
    public AudioClip cage_leave;

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

        SoundManager.Instance.PlaySound(cage_arrive, 0.8f);

        cage.PlayArrival();
    }

    public void Despawn()
    {
        Debug.Log("Visitor: leaving");
        SoundManager.Instance.PlaySound(cage_leave, 0.8f);
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


        if (currentCard.visitor != null && currentCard.visitor.arrivalSound != null)
        {
            SoundManager.Instance.PlaySound(currentCard.visitor.arrivalSound);
        }

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
