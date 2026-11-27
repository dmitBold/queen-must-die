using UnityEngine;
using UnityEngine.Rendering;

namespace NightCycle
{
    /// <summary>
    /// Пока объект включён, подменяет освещение окружения сцены: плоский ambient заданного цвета и включение тумана.
    /// Нужен участку, перенесённому из другой сцены, чтобы он выглядел как раньше. При выключении объекта
    /// или вызове Restore возвращает настройки сцены. Рендереры участка должны не брать свет из световых проб
    /// (Light Probes: Off), иначе запечённые пробы сцены всё равно подсветят их.
    /// </summary>
    public class RenderSettingsOverride : MonoBehaviour
    {
        /// <summary>Цвет плоского ambient на время подмены.</summary>
        [SerializeField] private Color _ambientColor = Color.black;
        /// <summary>Включён ли туман на время подмены.</summary>
        [SerializeField] private bool _fog;

        private bool _applied;
        private AmbientMode _ambientMode;
        private Color _ambientLight;
        private SphericalHarmonicsL2 _ambientProbe;
        private bool _sceneFog;

        private void OnEnable()
        {
            Apply();
        }

        private void OnDisable()
        {
            Restore();
        }

        /// <summary>Возвращает освещение сцены, не дожидаясь выключения объекта. Повторный вызов ничего не делает.</summary>
        public void Restore()
        {
            if (!_applied) return;
            _applied = false;

            RenderSettings.ambientMode = _ambientMode;
            RenderSettings.ambientLight = _ambientLight;
            RenderSettings.ambientProbe = _ambientProbe;
            RenderSettings.fog = _sceneFog;
        }

        private void Apply()
        {
            if (_applied) return;
            _applied = true;

            _ambientMode = RenderSettings.ambientMode;
            _ambientLight = RenderSettings.ambientLight;
            _ambientProbe = RenderSettings.ambientProbe;
            _sceneFog = RenderSettings.fog;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = _ambientColor;
            // Рендереры без проб берут ambient из ambientProbe; в сцене с запечённым светом он не пересчитывается от ambientLight.
            var probe = new SphericalHarmonicsL2();
            probe.AddAmbientLight(_ambientColor);
            RenderSettings.ambientProbe = probe;
            RenderSettings.fog = _fog;
        }
    }
}
