using System.Collections;
using Core;
using EasyTextEffects;
using TMPro;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class NightTypewriter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private float typingDelay = 0.05f;
        [SerializeField] private AudioClip typeSound;
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

        public void TypeText(string text)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            dialogueText.text = text;

            dialogueText.maxVisibleCharacters = 99999;
            dialogueText.ForceMeshUpdate();

            textEffectComponent.Refresh();

            currentVisibleCharacters = 0;

            typingCoroutine = StartCoroutine(TypeRoutine());
        }

        private IEnumerator TypeRoutine()
        {
            IsTyping = true;
            int totalCharacters = dialogueText.textInfo.characterCount;

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

        public void SkipTyping()
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            currentVisibleCharacters = dialogueText.textInfo.characterCount;
            IsTyping = false;
        }

        private void LateUpdate()
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