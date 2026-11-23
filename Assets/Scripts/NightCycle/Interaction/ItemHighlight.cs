using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Подсветка Important item рядом с предметом или запиской. Видна, пока предмет включён (activeSelf),
    /// и плавно гаснет навсегда, когда предмет выключается (подбор или восстановление из сохранения).
    /// Кладётся соседом предмета, а не внутрь: иначе Outline предмета обводит частицы.
    /// </summary>
    [RequireComponent(typeof(ParticleFadeOut))]
    public class ItemHighlight : MonoBehaviour
    {
        // WorldEventSaver восстанавливает подобранные предметы в Start или кадром позже:
        // выключение в эти кадры — загрузка, а не подбор, и подсветка пропадает сразу, без угасания.
        private const int RestoreGraceFrames = 3;

        /// <summary>Предмет, за которым следит подсветка. Без него подсветка горит всегда.</summary>
        [SerializeField] private GameObject _target;

        private ParticleSystem[] _systems;
        private bool _isShown;
        private int _framesSinceStart;

        private void Start()
        {
            _systems = GetComponentsInChildren<ParticleSystem>(true);
            _isShown = true;
            if (_target != null && !_target.activeSelf)
            {
                SetShown(false);
            }
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            _framesSinceStart++;
            bool targetActive = _target.activeSelf;
            if (targetActive == _isShown)
            {
                return;
            }

            if (targetActive)
            {
                SetShown(true);
            }
            else if (_framesSinceStart <= RestoreGraceFrames)
            {
                gameObject.SetActive(false);
            }
            else
            {
                _isShown = false;
                enabled = false;
                GetComponent<ParticleFadeOut>().FadeOut();
            }
        }

        // Пока предмет ещё не появился в сцене (выключен до события), частицы остановлены, а не выключены:
        // объект должен оставаться активным, чтобы Update заметил появление предмета.
        private void SetShown(bool shown)
        {
            _isShown = shown;
            foreach (ParticleSystem system in _systems)
            {
                if (shown)
                {
                    system.Play(false);
                }
                else
                {
                    system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }
}
