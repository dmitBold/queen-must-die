using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{
    public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI label;
        // ВНИМАНИЕ: Замени Image на RawImage в инспекторе!
        [SerializeField] private RawImage iconRaw;
        [SerializeField] private float hoverDelay = 0.5f;

        [Header("3D Preview Settings")]
        [SerializeField] private float rotationSpeed = 60f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -5f); // Отдаление камеры от предмета

        private InventoryUI inventoryUI;
        private ItemData item;
        private ItemTooltip tooltip;
        private Coroutine hoverRoutine;
        private bool isPartSlot;

        // Переменные для 3D рендера
        private GameObject previewScene;
        private Camera previewCamera;
        private RenderTexture renderTexture;
        private Transform modelTransform;

        // Статическая переменная, чтобы разносить превьюшки слотов далеко друг от друга
        private static int slotCounter = 0;

        public void Set(ItemData data, InventoryUI ui, bool canApply, ItemTooltip tooltipInstance, bool isPart = false)
        {
            item = data;
            inventoryUI = ui;
            tooltip = tooltipInstance;
            isPartSlot = isPart;

            if (label != null) label.text = data.itemName;

            if (tooltip != null) tooltip.Hide();
            gameObject.SetActive(true);

            // Настраиваем 3D превью
            Setup3DPreview(data);

            // Затемняем RawImage, если деталь не подходит к сокету
            iconRaw.color = canApply ? Color.white : new Color(1, 1, 1, 0.4f);
        }

        private void Setup3DPreview(ItemData data)
        {
            ClearPreview(); // Очищаем старое превью, если слот переиспользуется

            if (data.uiModelPrefab == null)
            {
                Debug.LogWarning($"[ItemSlot] У предмета {data.itemName} не назначен 3D префаб (uiModelPrefab)!");
                return;
            }

            // 1. Создаем RenderTexture (разрешение 256x256 идеально для пиксельного стиля)
            renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            iconRaw.texture = renderTexture;

            // 2. Создаем контейнер для мини-сцены глубоко под землей
            slotCounter++;
            Vector3 spawnPos = new Vector3(0, -10000f - (slotCounter * 50f), 0);
            previewScene = new GameObject($"PreviewScene_{data.id}");
            previewScene.transform.position = spawnPos;

            // 3. Настраиваем камеру превью
            GameObject camObj = new GameObject("PreviewCamera");
            camObj.transform.SetParent(previewScene.transform);
            camObj.transform.localPosition = cameraOffset;

            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.targetTexture = renderTexture;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0, 0, 0, 0); // Абсолютно прозрачный фон
            previewCamera.fieldOfView = 30f; // Узкий FOV для лучшего фокуса на предмете

            // Если твоя основная игра использует Culling Masks, здесь тоже можно настроить, 
            // но так как мы спавним это на -10000 по Y, основная камера это и так не увидит.

            // 4. Добавляем базовый свет (чтобы модели не были черными силуэтами)
            GameObject lightObj = new GameObject("PreviewLight");
            lightObj.transform.SetParent(previewScene.transform);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f; // Настрой под свои шейдеры
            lightObj.transform.rotation = Quaternion.Euler(45f, -45f, 0);
            int layerIndex = LayerMask.NameToLayer("Default");
            light.cullingMask = 1 << layerIndex;

            // 5. Спавним саму 3D модель
            GameObject modelInstance = Instantiate(data.uiModelPrefab, previewScene.transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localScale = data.uiModelScale;
            modelTransform = modelInstance.transform;
        }

        private void Update()
        {
            // Простая анимация вращения вокруг своей оси
            if (modelTransform != null)
            {
                modelTransform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item == null) return;
            hoverRoutine = StartCoroutine(HoverDelay());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopHover();
        }

        private IEnumerator HoverDelay()
        {
            yield return new WaitForSeconds(hoverDelay);
            if (item != null && tooltip != null)
            {
                tooltip.Show(item.description, transform.position + new Vector3(150, 0));
            }
        }

        private void StopHover()
        {
            if (hoverRoutine != null)
            {
                StopCoroutine(hoverRoutine);
                hoverRoutine = null;
            }
            if (tooltip != null) tooltip.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (item != null && inventoryUI != null)
            {
                inventoryUI.OnItemClicked(item, isPartSlot);
            }
        }

        private void ClearPreview()
        {
            if (previewScene != null) Destroy(previewScene);
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
            if (iconRaw != null) iconRaw.texture = null;
        }

        private void OnDisable()
        {
            StopHover();
        }

        private void OnDestroy()
        {
            ClearPreview(); // Обязательно чистим память при удалении слота
        }
    }
}