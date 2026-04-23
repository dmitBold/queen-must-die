using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    public class CodeLock : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int[] correctCombination = { 0, 1, 2 };
        [SerializeField] private LayerMask cylinderLayer;

        [Header("Cylinders")]
        [SerializeField] private List<CodeLockCylinder> cylinders;

        [Header("Events")]
        public UnityEvent OnUnlocked;

        private bool isFocused = false;
        private bool isUnlocked = false;

        //TEST
        public AudioClip unlockSound;
        public Camera cam;
        public string tutorialText;
        string og_text;
        public TextMeshProUGUI textArea;
        //TEST

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        private void Start()
        {
            foreach (var cylinder in cylinders)
            {
                cylinder.OnValueChanged += CheckCombination;
            }
            og_text = textArea.text;
        }

        public void Enter()
        {
            isFocused = true;
            textArea.text = tutorialText;
        }

        public void Exit()
        {
            isFocused = false;
            textArea.text = og_text;
        }

        void Update()
        {
            if (!isFocused || isUnlocked) return;

            // ��������� ����� �� ���������
            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
            for (int i = 0; i < cylinders.Count; i++)
            {
                Debug.Log(cylinders[i].CurrentValue);
            }
        }

        private void HandleClick()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, cylinderLayer))
            {
                if (hit.collider.TryGetComponent<CodeLockCylinder>(out var cylinder))
                {
                    cylinder.Rotate();
                }
            }
        }

        private void CheckCombination()
        {
            if (isUnlocked) return;

            for (int i = 0; i < cylinders.Count; i++)
            {
                if (cylinders[i].CurrentValue != correctCombination[i])
                    return; // ��� �� ������
            }

            Unlock();
        }

        private void Unlock()
        {
            isUnlocked = true;
            if (unlockSound != null)
            {
                _audioService.PlaySound(unlockSound);
            }
            Debug.Log("<color=green>����� ������! � ���� ������� ����!</color>");
            OnUnlocked?.Invoke();
            // ���� � ���������� ����� ����� �������� �������� �����, ���� ��� ������ ��������
        }
    }
}