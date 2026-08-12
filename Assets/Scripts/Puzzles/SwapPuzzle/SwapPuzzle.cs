using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NightCycle.Puzzles
{
    public class SwapPuzzle : FocusPuzzle
    {
        [Header("Swap Settings")]
        [SerializeField] private List<SwapItem> items;
        [SerializeField] private List<Transform> slots;
        [SerializeField] private float pullOutDistance = 0.5f; // Насколько предмет выдвигается
        [SerializeField] private float animationSpeed = 3f;

        private SwapItem hoverItem = null;
        private SwapItem firstSelectedItem = null;
        private bool isAnimating = false;

        private void Start()
        {
            LoadPuzzleState();
            if (HUDController.instance != null)
            {
                controller = HUDController.instance;
            }
        }

        private void Update()
        {
            if (!isFocused || isAnimating || isSolved) return;

            HandleMouseHover();
            // Обработка клика мыши
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }

        private void HandleMouseClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SwapItem clickedItem = hit.collider.GetComponent<SwapItem>();

                if (clickedItem != null && items.Contains(clickedItem))
                {
                    SelectOrSwapItem(clickedItem);
                }
            }
        }

        private void HandleMouseHover()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SwapItem HoverItem = hit.collider.GetComponent<SwapItem>();

                if (HoverItem != null && items.Contains(HoverItem))
                {
                    if (hoverItem == null)
                    {
                        if (HoverItem != firstSelectedItem)
                        {
                            hoverItem = HoverItem;
                            HoverItem.SetOutline(true);
                        }
                        else
                        {
                            hoverItem = null;
                        }
                    }
                    else
                    {
                        if (HoverItem != hoverItem && HoverItem != firstSelectedItem)
                        {
                            hoverItem.SetOutline(false);
                            hoverItem = HoverItem;
                            HoverItem.SetOutline(true);
                        }
                    }
                }
                else
                {
                    if (hoverItem != null)
                    {
                        hoverItem.SetOutline(false);
                    }
                    hoverItem = null;
                }
            }
        }

        private void SelectOrSwapItem(SwapItem item)
        {
            if (firstSelectedItem == null)
            {
                // Выделяем первый предмет
                firstSelectedItem = item;
                firstSelectedItem.SetOutline(false);
                StartCoroutine(PullOutRoutine(firstSelectedItem, true));
            }
            else if (firstSelectedItem == item)
            {
                // Снимаем выделение
                //firstSelectedItem.SetOutline(false);
                StartCoroutine(PullOutRoutine(firstSelectedItem, false));
                firstSelectedItem = null;
            }
            else
            {
                // Второй предмет выбран - меняем местами
                SwapItem secondSelectedItem = item;
                //secondSelectedItem.SetOutline(true);
                //firstSelectedItem.SetOutline(false);
                secondSelectedItem.SetOutline(false);

                StartCoroutine(SwapAnimationRoutine(firstSelectedItem, secondSelectedItem));
                firstSelectedItem = null;
            }
        }

        // Анимация выдвигания/задвигания одного предмета
        private IEnumerator PullOutRoutine(SwapItem item, bool isPullingOut)
        {
            isAnimating = true;
            Vector3 startPos = item.transform.position;
            Vector3 targetPos = slots[item.currentIndex].position + (isPullingOut ? transform.right * pullOutDistance : Vector3.zero);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * animationSpeed;
                item.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            isAnimating = false;
        }

        // Универсальная анимация обмена двух предметов
        private IEnumerator SwapAnimationRoutine(SwapItem item1, SwapItem item2)
        {
            isAnimating = true;

            int index1 = item1.currentIndex;
            int index2 = item2.currentIndex;

            Vector3 pos1Start = slots[index1].position + transform.right * pullOutDistance;
            Vector3 pos2Start = slots[index2].position;

            // Сначала выдвигаем вторую книгу
            float t = 0;
            Vector3 pos2PulledOut = pos2Start + transform.right * pullOutDistance;
            while (t < 1)
            {
                t += Time.deltaTime * animationSpeed;
                item2.transform.position = Vector3.Lerp(pos2Start, pos2PulledOut, t);
                yield return null;
            }

            // Перемещаем их по осям X/Y на места друг друга (оставаясь выдвинутыми)
            t = 0;
            Vector3 pos1Target = slots[index2].position + transform.right * pullOutDistance;
            Vector3 pos2Target = slots[index1].position + transform.right * pullOutDistance;

            while (t < 1)
            {
                t += Time.deltaTime * animationSpeed;
                item1.transform.position = Vector3.Lerp(pos1Start, pos1Target, t);
                item2.transform.position = Vector3.Lerp(pos2PulledOut, pos2Target, t);
                yield return null;
            }

            // Задвигаем обе книги обратно
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * animationSpeed;
                item1.transform.position = Vector3.Lerp(pos1Target, slots[index2].position, t);
                item2.transform.position = Vector3.Lerp(pos2Target, slots[index1].position, t);
                yield return null;
            }

            // Обновляем логические индексы
            item1.currentIndex = index2;
            item2.currentIndex = index1;

            isAnimating = false;
            CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            foreach (var item in items)
            {
                if (item.currentIndex != item.correctIndex) return; // Паззл еще не собран
            }

            SolvePuzzle();
        }

        protected override void SavePuzzleResult()
        {
            // Сохраняем факт выполнения через вашу систему ивентов
            var eventSaver = GetComponent<WorldEventSaver>();
            if (eventSaver != null) eventSaver.SaveEventState();
        }

        public override void LoadPuzzleState()
        {
            // Здесь будет логика загрузки индексов массива
        }

        public void exitPuzzle()
        {
            if (playerInteraction != null)
            {
                playerInteraction.ForceExitFocus();
            }
        }

    }
}