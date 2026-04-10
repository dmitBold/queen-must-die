using UnityEngine;
using UnityEngine.EventSystems;
using static CardManager;

public class ChoiceHoverHandler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    Choice choice;
    ChoiceImpactUI impactUI;
    CardManager cardManager;

    public void Init(Choice choice, ChoiceImpactUI impactUI, CardManager cardManager)
    {
        this.choice = choice;
        this.impactUI = impactUI;
        this.cardManager = cardManager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardManager.CanChoose(choice) != ChoiceAvailability.Available)
            return;

        impactUI.Show(choice);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //impactUI.Hide();
        impactUI.Hide_Anim();
    }
}
