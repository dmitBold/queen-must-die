using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Core;
using FMODUnity;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Ветка «Statues» (Office_statues активен): после сборки книжного паззла статуи кабинета оживают.
    /// Решён паззл -> неподвижные статуи заменяются активными -> после анимации тайного прохода дышит ближайшая к нему статуя ->
    /// через 1 с статуи активируются (как «Pickup attack»: идут к игроку, смертельный триггер, стоп-зона).
    /// Игрок ушёл в телепорт за проходом -> статуи Office 1–8 исчезают, проход закрывается, паззл заблокирован,
    /// статуэтка дальше запускает стандартную последовательность gear statue (обычная фигурка вместо альтернативной).
    /// Исчезновение сохраняется (SaveSystem) и восстанавливается после загрузки.
    /// </summary>
    public class OfficeSecretStatues : MonoBehaviour
    {
        [Header("Ветка")]
        [Tooltip("Office_statues: активен только в ветке Statues")]
        public GameObject officeStatues;
        public GameObject staticStatues;
        public GameObject activeStatues;

        [Header("Тайный проход")]
        public GameObject secretRoot;
        public Animator secretAnimator;
        public string openStateName = "Library secret enter";

        [Header("Оживление")]
        [Tooltip("Ближайшая к проходу статуя: на ней звучит дыхание после анимации")]
        public StatueBreath breather;
        [Tooltip("Звук дыхания")]
        public EventReference breathEvent;
        [Tooltip("Идущие статуи (как в Pickup attack): стартуют через activateDelay после конца анимации")]
        public List<StatueBreath> silentMovers = new List<StatueBreath>();
        public float activateDelay = 4f;
        [Tooltip("Сохранить игру (позиция игрока) сразу после открытия прохода")]
        public bool saveAfterOpen = true;
        [Tooltip("Включаются при активации (атак-триггер, стоп-зона)")]
        public List<GameObject> enableOnActivate = new List<GameObject>();
        [Tooltip("Стоящие статуи: avoidancePriority = 0, как в Pickup attack")]
        public List<NavMeshAgent> quietAgents = new List<NavMeshAgent>();

        [Header("Исчезновение")]
        [Tooltip("Выключаются вместе со статуями (смертельный триггер, стоп-зона, атак-триггер)")]
        public List<GameObject> disableOnVanish = new List<GameObject>();
        public Collider puzzleCollider;
        public Behaviour puzzleInteractable;
        public GameObject figureDefault;
        public GameObject figureAlt;

        [Tooltip("Уникальный id для сохранения")]
        public string saveId;

        private SaveSystem _save;
        private SaveManager _saveManager;
        private AudioService _audio;
        private bool _gone, _running;
        private string AwakeId => saveId + "_awake";

        [Inject] public void Construct(SaveSystem saveSystem, [InjectOptional] SaveManager saveManager, [InjectOptional] AudioService audio)
        { _save = saveSystem; _saveManager = saveManager; _audio = audio; }

        private IEnumerator Start()
        {
            if (_save == null || string.IsNullOrEmpty(saveId)) yield break;
            bool gone = _save.SessionTriggeredEvents.Contains(saveId), awake = _save.SessionTriggeredEvents.Contains(AwakeId);
            if (!gone && !awake) yield break;
            // после остальных Start (Day/Night Connection, WorldEventSaver, BookRowGate)
            yield return null; yield return null;
            if (gone) Vanish();
            else if (InBranch) StartCoroutine(Awaken(true));   // загрузка сохранения «проход открыт»: статуи снова оживают
        }

        private bool InBranch => officeStatues != null && officeStatues.activeInHierarchy;

        public void OnPuzzleSolved()
        {
            if (!InBranch || _gone || _running || (activeStatues && activeStatues.activeSelf)) return;
            StartCoroutine(Awaken(false));
        }

        private IEnumerator Awaken(bool fromSave)
        {
            _running = true;
            if (staticStatues) staticStatues.SetActive(false);
            if (activeStatues) activeStatues.SetActive(true);

            // ждём конца анимации открытия прохода
            float timeout = fromSave ? 0f : 6f;
            yield return null;
            while (timeout > 0f && secretAnimator && secretAnimator.isActiveAndEnabled)
            {
                var st = secretAnimator.GetCurrentAnimatorStateInfo(0);
                if (st.IsName(openStateName) && st.normalizedTime >= 1f && !secretAnimator.IsInTransition(0)) break;
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (_gone) yield break;
            if (!fromSave)
            {
                if (_save != null) _save.RegisterEvent(AwakeId);
                if (saveAfterOpen && _saveManager != null) _saveManager.SaveGame();   // точка = где стоит игрок
            }
            if (_audio != null && !breathEvent.IsNull) _audio.PlayFMODEvent(breathEvent, breather ? breather.transform.position : transform.position);

            yield return new WaitForSeconds(activateDelay);
            if (_gone) yield break;
            foreach (var go in enableOnActivate) if (go) go.SetActive(true);
            foreach (var a in quietAgents) if (a) a.avoidancePriority = 0;
            foreach (var s in silentMovers) if (s) s.BreathCompleted?.Invoke();
            _running = false;
        }

        public void OnTeleported()
        {
            if (!InBranch || _gone) return;
            if (_save != null && !string.IsNullOrEmpty(saveId)) _save.RegisterEvent(saveId);
            Vanish();
        }

        private void Vanish()
        {
            _gone = true;
            StopAllCoroutines(); _running = false;
            if (staticStatues) staticStatues.SetActive(false);
            if (activeStatues) activeStatues.SetActive(false);
            foreach (var go in disableOnVanish) if (go) go.SetActive(false);

            // проход — в положение до анимации
            if (secretRoot && !secretRoot.activeSelf) secretRoot.SetActive(true);
            if (secretAnimator) { secretAnimator.Rebind(); secretAnimator.Update(0f); }

            // паззл недоступен
            if (puzzleCollider) puzzleCollider.enabled = false;
            if (puzzleInteractable) puzzleInteractable.enabled = false;

            // статуэтка -> обычная фигурка (gear statue), если её ещё не подобрали
            if (figureAlt && figureAlt.activeSelf && figureDefault)
            {
                var altIt = figureAlt.GetComponent<Interactable>(); var defIt = figureDefault.GetComponent<Interactable>();
                figureAlt.SetActive(false);
                figureDefault.SetActive(true);
                if (altIt && defIt) defIt.enabled = altIt.enabled;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(saveId)) { saveId = System.Guid.NewGuid().ToString(); UnityEditor.EditorUtility.SetDirty(this); }
        }
#endif
    }
}
