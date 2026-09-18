using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VFXFadeController : MonoBehaviour
{
    public enum FadeMode
    {
        NaturalStop,    // Эмиттер просто перестает спавнить частицы (они доживают свой Lifetime)
        StandardScale,  // Объект плавно сжимается в 0
        ShaderDissolve  // Шейдерное растворение по частям
    }

    [Header("Settings")]
    [SerializeField] private FadeMode fadeMode = FadeMode.NaturalStop;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Shader Dissolve Settings")]
    [Tooltip("Рендерер материала, в котором есть параметр растворения")]
    [SerializeField] private Renderer vfxRenderer;
    [SerializeField] private string dissolvePropertyName = "_DissolveAmount";

    [Header("Events")]
    public UnityEvent OnFadeComplete;

    public List<ParticleSystem> particleSystems;
    private MaterialPropertyBlock propBlock;
    private int dissolvePropertyID;
    private Vector3 initialScale;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        dissolvePropertyID = Shader.PropertyToID(dissolvePropertyName);
        initialScale = transform.localScale;
    }

    public void TriggerFadeOut()
    {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // 1. Сразу отключаем эмиттеры, чтобы новые частицы не появлялись во время затухания
        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }

        float elapsed = 0f;

        // 2. Основной цикл плавного перехода
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float curveT = fadeCurve.Evaluate(t);

            switch (fadeMode)
            {
                case FadeMode.StandardScale:
                    transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, curveT);
                    break;

                case FadeMode.ShaderDissolve:
                    if (vfxRenderer != null)
                    {
                        // Архитектурно правильное изменение параметров без создания дубликатов материалов
                        vfxRenderer.GetPropertyBlock(propBlock);
                        propBlock.SetFloat(dissolvePropertyID, curveT);
                        vfxRenderer.SetPropertyBlock(propBlock);
                    }
                    break;

                case FadeMode.NaturalStop:
                    // Просто ждем, частицы сами исчезают благодаря Color/Size over Lifetime
                    break;
            }

            yield return null;
        }

        // 3. Жесткая очистка и выключение
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        OnFadeComplete?.Invoke();
        gameObject.SetActive(false);

        // 4. Сброс состояния (ОБЯЗАТЕЛЬНО для систем пулинга объектов)
        ResetState();
    }

    private void ResetState()
    {
        transform.localScale = initialScale;

        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = true; // Возвращаем эмиттеры для следующего использования
        }

        if (fadeMode == FadeMode.ShaderDissolve && vfxRenderer != null)
        {
            vfxRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat(dissolvePropertyID, 0f);
            vfxRenderer.SetPropertyBlock(propBlock);
        }
    }
}