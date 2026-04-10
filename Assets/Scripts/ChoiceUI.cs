using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.EventSystems;
using TMPro;
using System;
using Unity.VisualScripting;

public class ChoiceUI : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text leftText;
    [SerializeField] private TMP_Text rightText;

    public event Action<Choice> OnChoiceSelected;

    private Choice leftChoice;
    private Choice rightChoice;

    //test
    public System.Action OnEyeSelected;
    [SerializeField] Button eyeButton;

    //test
    [SerializeField] ChoiceImpactUI impactUI;
    [SerializeField] CardManager cardManager;
    //test

    void Awake()
    {
        leftButton.onClick.AddListener(() => Select(leftChoice));
        rightButton.onClick.AddListener(() => Select(rightChoice));
    }

    public void Show(Choice left, Choice right)
    {
        leftChoice = left;
        rightChoice = right;

        leftText.text = left.text;
        rightText.text = right.text;

        gameObject.SetActive(true);

        SetupHover(leftButton, left);
        SetupHover(rightButton, right);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        //test
        impactUI.Hide();
    }

    void Select(Choice choice)
    {
        OnChoiceSelected?.Invoke(choice);
    }

    public void Start()
    {
        gameObject.SetActive(false);
        //test
        eyeButton.onClick.AddListener(OnEyeClicked);
    }

    //test
    void OnEyeClicked()
    {
        OnEyeSelected?.Invoke();
    }

    //test
    public void SetChoiceAvailability(bool leftAvailable, bool rightAvailable)
    {
        leftButton.interactable = leftAvailable;
        rightButton.interactable = rightAvailable;
    }


    //test
    void SetupHover(Button button, Choice choice)
    {
        /*if (!button.interactable)
        {
            return;
        }*/

        var hover = button.GetComponent<ChoiceHoverHandler>();
        if (hover == null)
            hover = button.gameObject.AddComponent<ChoiceHoverHandler>();
        hover.Init(choice, impactUI, cardManager);
    }
    //test


}
