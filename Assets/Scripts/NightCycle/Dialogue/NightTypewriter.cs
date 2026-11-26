using System;
using System.Collections;
using Core;
using EasyTextEffects;
using FMODUnity;
using TMPro;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class NightTypewriter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private float typingDelay = 0;
        [SerializeField] private EventReference defaulTtypeSound;
        [SerializeField] private EventReference typeSound;
        [SerializeField] private string TextingParameterName = "IsTyping";
        public TextEffect textEffectComponent;

        private Coroutine typingCoroutine;
        private int currentVisibleCharacters = 0;

        public bool IsTyping { get; private set; }

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void SetTypeSound(EventReference _typeSound)
        {
            if (!_typeSound.IsNull)
            {
                typeSound = _typeSound;
            }
            else
            {
                typeSound = defaulTtypeSound;
            }
        }

        public void TypeText(string text)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            // Обнуляем до смены текста: перестройка меша сразу прячет ещё не напечатанные символы.
            currentVisibleCharacters = 0;

            dialogueText.text = text;

            dialogueText.maxVisibleCharacters = 99999;
            dialogueText.ForceMeshUpdate();

            textEffectComponent.Refresh();

            typingCoroutine = StartCoroutine(TypeRoutine());
        }

        private IEnumerator TypeRoutine()
        {
            IsTyping = true;
            RuntimeManager.StudioSystem.setParameterByName(TextingParameterName, 1);
            _audioService.PlayFMODEvent(typeSound);
            int totalCharacters = dialogueText.textInfo.characterCount;

            while (currentVisibleCharacters < totalCharacters)
            {
                currentVisibleCharacters++;

                if (/*typeSound != null &&*/ currentVisibleCharacters % 2 == 0)
                {
                   // _audioService.PlayFMODEvent(typeSound);
                }

                yield return new WaitForSeconds(typingDelay);
            }

            IsTyping = false;
            
            RuntimeManager.StudioSystem.setParameterByName(TextingParameterName, 0);
        }

        public void SkipTyping()
        {

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            currentVisibleCharacters = dialogueText.textInfo.characterCount;
            IsTyping = false;
            RuntimeManager.StudioSystem.setParameterByName(TextingParameterName, 0);
        }

        public void StopTyping()
        {

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            currentVisibleCharacters = 0;
            IsTyping = false;
            RuntimeManager.StudioSystem.setParameterByName(TextingParameterName, 0);
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextMeshRebuilt);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextMeshRebuilt);
        }

        // TMP перестраивает меш и вне LateUpdate (при включении панели — во время отрисовки канваса,
        // EasyTextEffects — в своём Update) и возвращает всем символам полную альфу.
        // Без повторного скрытия весь текст на кадр вспыхивает в пустом окне.
        private void OnTextMeshRebuilt(UnityEngine.Object obj)
        {
            if (obj == dialogueText) HideUntypedCharacters();
        }

        private void LateUpdate()
        {
            HideUntypedCharacters();
        }

        private void HideUntypedCharacters()
        {
            if (dialogueText.textInfo == null || dialogueText.textInfo.characterCount == 0) return;

            if (currentVisibleCharacters >= dialogueText.textInfo.characterCount && !IsTyping) return;

            TMP_TextInfo textInfo = dialogueText.textInfo;

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

            dialogueText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}