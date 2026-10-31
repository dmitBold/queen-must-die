using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public float turnOFF_time;
    public float turnON_time;
    public List<Light> AllLight;

    private Dictionary<Light, float> _originalIntensities = new Dictionary<Light, float>();
    private Dictionary<Light, Coroutine> _lightCoroutines = new Dictionary<Light, Coroutine>();

    // Хранит состояние блокировки для каждого источника света
    private HashSet<Light> _lockedLights = new HashSet<Light>();

    private void Start()
    {
        foreach (Light light in AllLight)
        {
            if (light != null)
            {
                _originalIntensities[light] = light.intensity;
            }
        }
    }

    /// <summary>
    /// Публичный метод проверки: занят ли свет процессом выключения
    /// </summary>
    public bool IsLightLocked(Light light)
    {
        return _lockedLights.Contains(light);
    }

    public void ChangeLightIntensity(Light light, float targetIntensity, float duration)
    {
        if (light == null) return;

        // Если свет заблокирован (выключается), другие скрипты не могут изменить его интенсивность
        if (_lockedLights.Contains(light)) return;

        if (_lightCoroutines.TryGetValue(light, out Coroutine existingCoroutine) && existingCoroutine != null)
        {
            StopCoroutine(existingCoroutine);
        }

        _lightCoroutines[light] = StartCoroutine(ChangeLightIntensityRoutine(light, targetIntensity, duration, false));
    }

    private IEnumerator ChangeLightIntensityRoutine(Light light, float targetIntensity, float duration, bool disableAtEnd)
    {
        if (duration <= 0f)
        {
            light.intensity = targetIntensity;
            if (disableAtEnd) light.enabled = false;
            yield break;
        }

        float startIntensity = light.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        light.intensity = targetIntensity;

        // Выключаем свет только в самом конце анимации затухания
        if (disableAtEnd)
        {
            light.enabled = false;
            _lockedLights.Remove(light); // Снимаем блокировку после полного выключения
        }
    }

    public void TurnOffAllLight()
    {
        foreach (Light light in AllLight)
        {
            if (light == null) continue;

            // Прерываем текущую корутину, если она была
            if (_lightCoroutines.TryGetValue(light, out Coroutine existingCoroutine) && existingCoroutine != null)
            {
                StopCoroutine(existingCoroutine);
            }

            // Блокируем свет для других скриптов
            _lockedLights.Add(light);

            // Запускаем выключение с флагом disableAtEnd = true
            _lightCoroutines[light] = StartCoroutine(ChangeLightIntensityRoutine(light, 0f, turnOFF_time, true));
        }
    }

    public void TurnOnAllLight()
    {
        foreach (Light light in AllLight)
        {
            if (light != null && _originalIntensities.TryGetValue(light, out float origIntensity))
            {
                // При принудительном включении снимаем блокировку выключения
                _lockedLights.Remove(light);

                if (_lightCoroutines.TryGetValue(light, out Coroutine existingCoroutine) && existingCoroutine != null)
                {
                    StopCoroutine(existingCoroutine);
                }

                light.enabled = true; // Включаем сразу, чтобы была видна анимация проявления
                _lightCoroutines[light] = StartCoroutine(ChangeLightIntensityRoutine(light, origIntensity, turnON_time, false));
            }
        }
    }
}
