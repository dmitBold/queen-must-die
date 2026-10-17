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
        // ��������: ������ Image �� RawImage � ����������!
        [SerializeField] private RawImage iconRaw;
        [SerializeField] private float hoverDelay = 0.5f;

        [Header("3D Preview Settings")]
        [SerializeField] private float rotationSpeed = 60f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -5f); // ��������� ������ �� ��������

        private InventoryUI inventoryUI;
        private ItemData item;
        private ItemTooltip tooltip;
        private Coroutine hoverRoutine;
        private bool isPartSlot;

        // ���������� ��� 3D �������
        private GameObject previewScene;
        private Camera previewCamera;
        private RenderTexture renderTexture;
        private Transform modelTransform;

        // ����������� ����������, ����� ��������� ��������� ������ ������ ���� �� �����
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

            // ����������� 3D ������
            Setup3DPreview(data);

            // ��������� RawImage, ���� ������ �� �������� � ������
            iconRaw.color = canApply ? Color.white : new Color(1, 1, 1, 0.4f);
        }

        private void Setup3DPreview(ItemData data)
        {
            ClearPreview(); // ������� ������ ������, ���� ���� ����������������

            if (data.uiModelPrefab == null)
            {
                Debug.LogWarning($"[ItemSlot] � �������� {data.itemName} �� �������� 3D ������ (uiModelPrefab)!");
                return;
            }

            // 1. ������� RenderTexture (���������� 256x256 �������� ��� ����������� �����)
            renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            iconRaw.texture = renderTexture;

            // 2. ������� ��������� ��� ����-����� ������� ��� ������
            slotCounter++;
            Vector3 spawnPos = new Vector3(0, -10000f - (slotCounter * 50f), 0);
            previewScene = new GameObject($"PreviewScene_{data.id}");
            previewScene.transform.position = spawnPos;

            // 3. ����������� ������ ������
            GameObject camObj = new GameObject("PreviewCamera");
            camObj.transform.SetParent(previewScene.transform);
            camObj.transform.localPosition = cameraOffset;

            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.targetTexture = renderTexture;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0, 0, 0, 0); // ��������� ���������� ���
            previewCamera.fieldOfView = 30f; // ����� FOV ��� ������� ������ �� ��������

            // ���� ���� �������� ���� ���������� Culling Masks, ����� ���� ����� ���������, 
            // �� ��� ��� �� ������� ��� �� -10000 �� Y, �������� ������ ��� � ��� �� ������.

            // 4. ��������� ������� ���� (����� ������ �� ���� ������� ���������)
            GameObject lightObj = new GameObject("PreviewLight");
            lightObj.transform.SetParent(previewScene.transform);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f; // ������� ��� ���� �������
            light.cullingMask = LayerMask.GetMask("Assembly");
            lightObj.transform.rotation = Quaternion.Euler(45f, -45f, 0);
            int layerIndex = LayerMask.NameToLayer("Default");
            light.cullingMask = 1 << layerIndex;

            // 5. ������� ���� 3D ������
            GameObject modelInstance = Instantiate(data.uiModelPrefab, previewScene.transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localScale = data.uiModelScale;
            modelTransform = modelInstance.transform;
        }

        private void Update()
        {
            // ������� �������� �������� ������ ����� ���
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
            ClearPreview(); // ����������� ������ ������ ��� �������� �����
        }
    }
}