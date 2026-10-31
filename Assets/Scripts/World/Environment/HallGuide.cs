using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Холл ночью: огни ведут игрока к сломанному голему.
    /// Старт — в холле горят только окна и факелы у двери в спальню (всё из steps / finalFires / finalLights погашено).
    /// StartGuide() (подобрана эссенция стражника) — загорается steps[0] и тусклый свет над големом.
    /// Reach(k) (игрок дошёл до steps[k-1]) — загорается steps[k]. Последний шаг — факелы у голема.
    /// Через finalDelay после последнего шага за finalDuration разгорается весь остальной свет холла.
    /// Прогресс сохраняется через SaveSystem (события "{saveId}_1".."{saveId}_done").
    /// </summary>
    public class HallGuide : MonoBehaviour
    {
        [Serializable]
        public class Step
        {
            public string name;
            public List<FireIgnition> fires = new List<FireIgnition>();
        }

        public List<Step> steps = new List<Step>();

        [Header("Свет над големом")]
        public Light guideLight;
        public float guideLightFade = 2f;

        [Header("Весь остальной свет холла")]
        public List<FireIgnition> finalFires = new List<FireIgnition>();
        public List<Light> finalLights = new List<Light>();
        public float finalDelay = 2f;
        public float finalDuration = 4f;

        [Header("Сохранение")]
        public string saveId = "HallGuide";

        /// <summary>0 — ещё не начато, 1..steps.Count — сколько шагов зажжено.</summary>
        public int Stage { get; private set; }
        public bool Finished { get; private set; }

        private SaveSystem _saveSystem;
        private float[] _finalIntensity;
        private float _guideIntensity;

        [Inject]
        public void Construct(SaveSystem saveSystem) => _saveSystem = saveSystem;

        private void Awake()
        {
            _finalIntensity = new float[finalLights.Count];
            for (int i = 0; i < finalLights.Count; i++) _finalIntensity[i] = finalLights[i] ? finalLights[i].intensity : 0f;
            if (guideLight) _guideIntensity = guideLight.intensity;
        }

        private IEnumerator Start()
        {
            Stage = 0; Finished = false;
            if (_saveSystem != null)
            {
                if (_saveSystem.SessionTriggeredEvents.Contains(saveId + "_done")) { Stage = steps.Count; Finished = true; }
                else for (int k = steps.Count; k >= 1; k--) if (_saveSystem.SessionTriggeredEvents.Contains(saveId + "_" + k)) { Stage = k; break; }
            }
            ApplyInstant();
            yield return null;   // second pass: fire sounds / particles started in their own Start
            ApplyInstant();
            if (Stage == steps.Count && !Finished) StartCoroutine(FinalRoutine());
        }

        /// <summary>Эссенция стражника подобрана.</summary>
        public void StartGuide()
        {
            if (Stage != 0) return;
            Advance();
            if (guideLight) StartCoroutine(FadeLight(guideLight, 0f, _guideIntensity, guideLightFade));
        }

        /// <summary>Игрок дошёл до шага step (1 — первая пара жаровен ...). Зажигает следующий шаг.</summary>
        public void Reach(int step)
        {
            if (Stage != step || Stage >= steps.Count) return;
            Advance();
        }

        private void Advance()
        {
            var s = steps[Stage];
            foreach (var f in s.fires) if (f) f.Ignite();
            Stage++;
            Register(saveId + "_" + Stage);
            if (Stage == steps.Count) StartCoroutine(FinalRoutine());
        }

        private IEnumerator FinalRoutine()
        {
            yield return new WaitForSeconds(finalDelay);
            // огни разгораются вразнобой в течение finalDuration, лампы плавно набирают яркость
            var order = new List<FireIgnition>(finalFires);
            for (int i = 0; i < order.Count; i++) { int j = UnityEngine.Random.Range(i, order.Count); (order[i], order[j]) = (order[j], order[i]); }
            float span = Mathf.Max(0f, finalDuration - 1.2f);
            float t = 0f; int next = 0;
            foreach (var l in finalLights) if (l) { l.intensity = 0f; l.enabled = true; }
            while (t < finalDuration)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / finalDuration);
                for (int i = 0; i < finalLights.Count; i++) if (finalLights[i]) finalLights[i].intensity = _finalIntensity[i] * k;
                while (next < order.Count && t >= span * next / Mathf.Max(1, order.Count)) { if (order[next]) order[next].Ignite(); next++; }
                t += Time.deltaTime;
                yield return null;
            }
            for (; next < order.Count; next++) if (order[next]) order[next].Ignite();
            for (int i = 0; i < finalLights.Count; i++) if (finalLights[i]) finalLights[i].intensity = _finalIntensity[i];
            Finished = true;
            Register(saveId + "_done");
        }

        private void ApplyInstant()
        {
            for (int i = 0; i < steps.Count; i++)
                foreach (var f in steps[i].fires) if (f) f.SetLit(i < Stage);
            foreach (var f in finalFires) if (f) f.SetLit(Finished);
            for (int i = 0; i < finalLights.Count; i++)
                if (finalLights[i]) { finalLights[i].intensity = _finalIntensity[i]; finalLights[i].enabled = Finished; }
            if (guideLight) { guideLight.intensity = _guideIntensity; guideLight.enabled = Stage > 0; }
        }

        private IEnumerator FadeLight(Light l, float from, float to, float dur)
        {
            l.enabled = true;
            for (float t = 0f; t < dur; t += Time.deltaTime) { l.intensity = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / dur)); yield return null; }
            l.intensity = to;
        }

        private void Register(string id) { _saveSystem?.RegisterEvent(id); }

        [ContextMenu("Test: start guide")] private void TestStart() => StartGuide();
        [ContextMenu("Test: next step")] private void TestNext() => Reach(Stage);
    }
}
