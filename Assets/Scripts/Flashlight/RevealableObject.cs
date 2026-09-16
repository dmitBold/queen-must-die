using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class RevealableObject : MonoBehaviour
    {
        [Inject] private PlayerFlashlight flashlight;

        [Header("Detection Settings")]
        [Tooltip("Точка, которую должен осветить игрок (по умолчанию центр объекта)")]
        [SerializeField] private Transform detectionPoint;
        [Tooltip("Слои, блокирующие свет (стены, пол)")]
        [SerializeField] private LayerMask obstacleLayers;

        [Header("Interaction & Reveal")]
        [Tooltip("Компоненты, которые включатся при свете (Collider, Outline, Interactable)")]
        [SerializeField] private Behaviour[] componentsToToggle;
        [SerializeField] private GameObject[] ObjectsToToggle;
        [Tooltip("Обычный материал, на который заменится скрытый при начале взаимодействия")]
        [SerializeField] private Material standardMaterial;

        private bool isCurrentlyVisible = false;
        private bool isPermanentlyRevealed = false; // Блокировка при взаимодействии

        private float checkTimer = 0f;
        private readonly float checkInterval = 0.1f; // Проверка 10 раз в секунду (оптимизация)

        private void Start()
        {
            if (detectionPoint == null) detectionPoint = transform;
            SetComponentsState(false);
        }

        private void Update()
        {
            if (isPermanentlyRevealed) return; // Если уже взаимодействуем — логика отключается

            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            bool visibleThisFrame = CheckIfInLightCone();

            // Меняем стейт компонентов только в момент переключения (чтобы не дергать их постоянно)
            if (visibleThisFrame != isCurrentlyVisible)
            {
                isCurrentlyVisible = visibleThisFrame;
                SetComponentsState(isCurrentlyVisible);
            }
        }

        private bool CheckIfInLightCone()
        {
            if (!flashlight.light_active) return false;

            Vector3 lightOrigin = flashlight.GetVirtualOrigin();
            Vector3 dirToTarget = detectionPoint.position - lightOrigin;
            float distance = dirToTarget.magnitude;

            // 1. Проверка дистанции
            if (distance > flashlight.RevealDistance) return false;

            // 2. Проверка угла конуса
            Vector3 normalizedDir = dirToTarget / distance;
            float angle = Vector3.Angle(flashlight.flashlight.transform.forward, normalizedDir);
            if (angle > flashlight.RevealAngle) return false;

            // 3. Проверка препятствий (Raycast)
            if (Physics.Raycast(lightOrigin, normalizedDir, distance, obstacleLayers)) return false;

            return true;
        }

        private void SetComponentsState(bool state)
        {
            foreach (var comp in componentsToToggle)
            {
                if (comp != null) comp.enabled = state;
            }

            foreach (var obj in ObjectsToToggle)
            {
                if (obj != null) obj.SetActive(state);
            }
        }

        // ВЫЗЫВАТЬ ЧЕРЕЗ ТВОЙ СКРИПТ ВЗАИМОДЕЙСТВИЯ ПРИ КЛИКЕ ИГРОКА
        public void LockInteraction()
        {
            isPermanentlyRevealed = true;
            SetComponentsState(true);

            // Мгновенно подменяем материал на стандартный, чтобы модель больше не зависела от шейдера
            if (standardMaterial != null && TryGetComponent<Renderer>(out Renderer rend))
            {
                rend.material = standardMaterial;
            }
        }
    }
}