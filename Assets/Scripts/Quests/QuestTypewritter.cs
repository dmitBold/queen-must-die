using System.Collections;
using Core;
using EasyTextEffects;
using TMPro;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class QuestTypewriter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI FieldText;
        [SerializeField] private float typingDelay = 0;
        [SerializeField] private AudioClip typeSound;
        public TextEffect textEffectComponent;

        private Coroutine typingCoroutine;
        private int currentVisibleCharacters = 0;

        public bool IsTyping { get; private set; }
        public bool IsFading { get; private set; }

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        //метод печати
        public void TypeText(string text)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            SetAlphaInstant(1f);
            FieldText.text = text;
            FieldText.maxVisibleCharacters = 99999;

            FieldText.ForceMeshUpdate();

            if (textEffectComponent != null) textEffectComponent.Refresh();

            currentVisibleCharacters = 0;

            typingCoroutine = StartCoroutine(TypeRoutine());
        }

        private IEnumerator TypeRoutine()
        {
            IsTyping = true;
            int totalCharacters = FieldText.textInfo.characterCount;

            while (currentVisibleCharacters < totalCharacters)
            {
                currentVisibleCharacters++;

                if (typeSound != null && currentVisibleCharacters % 2 == 0)
                {
                    _audioService.PlaySound(typeSound);
                }

                yield return new WaitForSeconds(typingDelay);
            }

            IsTyping = false;

        }


        private void LateUpdate()
        {
            if (FieldText.textInfo == null || FieldText.textInfo.characterCount == 0) return;
            if (currentVisibleCharacters >= FieldText.textInfo.characterCount && !IsTyping) return;

            TMP_TextInfo textInfo = FieldText.textInfo;

            for (int i = currentVisibleCharacters; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

                vertexColors[vertexIndex + 0].a = 0;
                vertexColors[vertexIndex + 1].a = 0;
                vertexColors[vertexIndex + 2].a = 0;
                vertexColors[vertexIndex + 3].a = 0;
            }

            FieldText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        // --- МЕТОДЫ ДЛЯ АНИМАЦИЙ ---

        // Мгновенно выводит текст
        public void SetTextInstant(string text)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            IsTyping = false;
            FieldText.text = text;
            FieldText.maxVisibleCharacters = 99999;
            FieldText.ForceMeshUpdate();
            currentVisibleCharacters = FieldText.textInfo.characterCount; // Делаем всё видимым для LateUpdate
        }

        public void SetAlphaInstant(float alpha)
        {
            Color c = FieldText.color;
            FieldText.color = new Color(c.r, c.g, c.b, alpha);
        }

        public void StrikethroughText()
        {
            // Оборачиваем текущий текст в тег зачёркивания
            FieldText.text = $"<s>{FieldText.text}</s>";
        }

        public void ClearText()
        {
            FieldText.text = string.Empty;
        }

        // Универсальная корутина Fade
        public IEnumerator FadeTo(float targetAlpha, float duration)
        {
            IsFading = true;
            Color startColor = FieldText.color;
            float startAlpha = startColor.a;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                FieldText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            FieldText.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
            IsFading = false;
        }
    }
}