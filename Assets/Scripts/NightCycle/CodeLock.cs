using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

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

    void Start()
    {
        foreach (var cylinder in cylinders)
        {
            cylinder.OnValueChanged += CheckCombination;
        }
        og_text = textArea.text;
    }

    public void Enter()
    {
        Debug.Log("RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR");
        isFocused = true;
        //HUDController.instance.EnableInteractionText("Кликай по цифрам. E - выйти");
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

        // Обработка клика по цилиндрам
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
                return; // Код не совпал
        }

        Unlock();
    }

    private void Unlock()
    {
        isUnlocked = true;
        if (unlockSound != null)
        {
            SoundManager.Instance.PlaySound(unlockSound);
        }
        Debug.Log("<color=green>Замок открыт! У меня толстый член!</color>");
        OnUnlocked?.Invoke();
        // Сюда в инспекторе можно будет повесить открытие двери, звук или выдачу предмета
    }
}