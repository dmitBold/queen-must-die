using Cards;
using System;
//using Day_Docs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Choices
{
    public class PageChoiceUI : MonoBehaviour
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TMP_Text leftText;
        [SerializeField] private TMP_Text rightText;

        public event Action<PageChoice> OnChoiceSelected;

        private PageChoice leftChoice;
        private PageChoice rightChoice;


        [SerializeField] PagesManager pagesManager;

        void Awake()
        {
            leftButton.onClick.AddListener(() => Select(leftChoice));
            rightButton.onClick.AddListener(() => Select(rightChoice));
        }

        public void Show(PageChoice left, PageChoice right)
        {
            leftChoice = left;
            rightChoice = right;

            leftText.text = left.text;
            rightText.text = right.text;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);

        }

        void Select(PageChoice choice)
        {
            OnChoiceSelected?.Invoke(choice);
        }

        public void Start()
        {
            gameObject.SetActive(false);
        }


    }
}
