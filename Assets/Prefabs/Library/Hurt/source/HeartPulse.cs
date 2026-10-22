using UnityEngine;

[RequireComponent(typeof(Transform))]
public class HeartPulse : MonoBehaviour
{
    [Header("Пульсация (масштаб)")]
    [Range(0.5f, 1f)]
    public float minScale = 0.85f;

    [Range(0.2f, 3f)]
    public float beatsPerSecond = 1f;

    [Header("Свечение (Light)")]
    public Light heartLight;
    public float maxLightIntensity = 5f;
    public Color glowColor = new Color(1f, 0.1f, 0.1f);

    [Header("Свечение материала (URP Emission)")]
    public bool controlMaterialEmission = true;
    public Renderer heartRenderer;
    public float maxEmissionIntensity = 3f;

    [Header("Звук (FMOD)")]
    public FMODUnity.EventReference BaseHeartBeatEvent;

    [Tooltip("Длина звука стука в секундах — защита от наложения")]
    public float soundLength = 0.5f;

    private Vector3 _baseScale;
    private MaterialPropertyBlock _mpb;
    private float _phase;
    private bool _hasBeaten;
    private float _lastBeatTime = -10f;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        _baseScale = transform.localScale;
        _mpb = new MaterialPropertyBlock();

        if (heartLight != null)
        {
            heartLight.color = glowColor;
            heartLight.intensity = 0f;
            heartLight.type = LightType.Point;
        }

        if (heartRenderer == null)
            heartRenderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        _phase += Time.deltaTime * beatsPerSecond;
        if (_phase > 1f) _phase -= 1f;

        float pulseCurve = Mathf.Sin(_phase * Mathf.PI * 2f);
        float compress = Mathf.Max(0f, pulseCurve);

        if (_phase < 0.5f)
        {
            if (!_hasBeaten && Time.time - _lastBeatTime >= soundLength)
            {
                FMODUnity.RuntimeManager.PlayOneShot(BaseHeartBeatEvent, transform.position);
                _lastBeatTime = Time.time;
                _hasBeaten = true;
            }
        }
        else
        {
            _hasBeaten = false;
        }

        float scaleFactor = Mathf.Lerp(1f, minScale, compress);
        transform.localScale = _baseScale * scaleFactor;

        if (heartLight != null)
        {
            heartLight.intensity = compress * maxLightIntensity;
        }

        if (controlMaterialEmission && heartRenderer != null)
        {
            heartRenderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissionColorID, glowColor * (compress * maxEmissionIntensity));
            heartRenderer.SetPropertyBlock(_mpb);
        }
    }
}