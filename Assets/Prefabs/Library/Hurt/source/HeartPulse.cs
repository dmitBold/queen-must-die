using UnityEngine;

[RequireComponent(typeof(Transform))]
public class HeartPulse : MonoBehaviour
{
    [Header("Пульсация (масштаб)")]
    [Tooltip("Во сколько раз сжимается сердце (0.8 = на 20% меньше)")]
    [Range(0.5f, 1f)]
    public float minScale = 0.85f;

    [Tooltip("Скорость одного удара (циклов в секунду)")]
    [Range(0.2f, 3f)]
    public float beatsPerSecond = 1f;

    [Header("Свечение (Light)")]
    [Tooltip("Источник света внутри сердца")]
    public Light heartLight;

    [Tooltip("Максимальная интенсивность света при сжатии")]
    public float maxLightIntensity = 5f;

    [Tooltip("Цвет свечения")]
    public Color glowColor = new Color(1f, 0.1f, 0.1f);

    [Header("Свечение материала (URP Emission)")]
    [Tooltip("Автоматически менять emission у материала?")]
    public bool controlMaterialEmission = true;

    [Tooltip("Рендерер сердца")]
    public Renderer heartRenderer;

    [Tooltip("Максимальная яркость emission")]
    public float maxEmissionIntensity = 3f;

    // Внутренние переменные
    private Vector3 _baseScale;
    private MaterialPropertyBlock _mpb;
    private float _phase;

    // ID свойства emission для URP
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        _baseScale = transform.localScale;
        _mpb = new MaterialPropertyBlock();

        // Настраиваем свет
        if (heartLight != null)
        {
            heartLight.color = glowColor;
            heartLight.intensity = 0f;
            heartLight.type = LightType.Point;
        }

        // Если рендерер не задан — берём с этого же объекта
        if (heartRenderer == null)
            heartRenderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        // Фаза пульсации от 0 до 1 (синусоида)
        _phase += Time.deltaTime * beatsPerSecond;
        if (_phase > 1f) _phase -= 1f;

        // Кривая пульса: быстрый "удар" и плавное возвращение
        // t = 0 → нормальный размер, t = 1 → нормальный размер, середина → сжатие
        float pulseCurve = Mathf.Sin(_phase * Mathf.PI * 2f); // -1..1
        // Преобразуем в 0..1, где 1 = максимальное сжатие
        float compress = Mathf.Max(0f, pulseCurve);

        // Масштаб
        float scaleFactor = Mathf.Lerp(1f, minScale, compress);
        transform.localScale = _baseScale * scaleFactor;

        // Свет
        if (heartLight != null)
        {
            heartLight.intensity = compress * maxLightIntensity;
        }

        // Emission материала
        if (controlMaterialEmission && heartRenderer != null)
        {
            heartRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionColorID, glowColor * (compress * maxEmissionIntensity));
            heartRenderer.SetPropertyBlock(_mpb);
        }
    }
}