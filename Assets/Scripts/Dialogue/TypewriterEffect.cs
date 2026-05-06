using System.Collections;
using TMPro;
using UnityEngine;
using EasyTextEffects;

namespace Dialogue
{
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField] float delay = 0.03f;

        public TextEffect textEffectComponent;

        TextMeshProUGUI field;
        string[] pages;
        int pageIndex;
        int maxPageIndexSeen = -1;

        int currentVisibleCharacters;
        Coroutine routine;
        bool isTyping;

        public bool IsFinished => !isTyping && (pages == null || pageIndex >= pages.Length - 1);
        public bool CanGoBack => pageIndex > 0;

        public event System.Action OnDialogueFinished;
        public event System.Action<int> OnPageFinished;
        public event System.Action OnDialogueBack;

        public enum SkipResult
        {
            None,
            PageFinished,
            DialogueFinished
        }

        public void Play(TextMeshProUGUI text, string[] contentPages)
        {
            if (routine != null) StopCoroutine(routine);

            field = text;
            pages = contentPages;
            pageIndex = 0;
            maxPageIndexSeen = -1;

            ShowCurrentPage();
        }

        void ShowCurrentPage()
        {
            if (pages == null || pages.Length == 0) return;

            if (routine != null) StopCoroutine(routine);

            field.text = pages[pageIndex];
            field.maxVisibleCharacters = 99999;
            field.ForceMeshUpdate();

            if (textEffectComponent != null) textEffectComponent.Refresh();

            int totalCharacters = field.textInfo.characterCount;

            if (delay <= 0f || pageIndex <= maxPageIndexSeen)
            {
                currentVisibleCharacters = totalCharacters;
                isTyping = false;
                UpdateVertexVisibility();

                if (IsFinished)
                {
                    OnDialogueFinished?.Invoke();
                }
                else
                {
                    OnPageFinished?.Invoke(pageIndex);
                }

            }
            else
            {
                maxPageIndexSeen = pageIndex;
                currentVisibleCharacters = 0;
                isTyping = true;
                UpdateVertexVisibility();
                routine = StartCoroutine(TypeRoutine());
            }
        }

        IEnumerator TypeRoutine()
        {
            int totalCharacters = field.textInfo.characterCount;

            while (currentVisibleCharacters < totalCharacters)
            {
                currentVisibleCharacters++;
                yield return new WaitForSeconds(delay);
            }

            isTyping = false;

            if (IsFinished)
            {
                OnDialogueFinished?.Invoke();
            }
            else
            {
                OnPageFinished?.Invoke(pageIndex);
            }
        }

        public SkipResult Skip()
        {
            if (pages == null || pages.Length == 0) return SkipResult.None;

            if (isTyping)
            {
                if (routine != null) StopCoroutine(routine);
                currentVisibleCharacters = field.textInfo.characterCount;
                isTyping = false;
                UpdateVertexVisibility();

                if (IsFinished)
                {
                    OnDialogueFinished?.Invoke();
                }
                else
                {
                    OnPageFinished?.Invoke(pageIndex);
                }

                return SkipResult.PageFinished;
            }

            if (!IsFinished)
            {
                pageIndex++;
                ShowCurrentPage();
                return SkipResult.PageFinished;
            }

            return SkipResult.DialogueFinished;
        }

        public void back()
        {
            if (isTyping)
            {
                if (routine != null) StopCoroutine(routine);
                isTyping = false;
            }

            if (pageIndex <= 0) return;

            pageIndex--;
            ShowCurrentPage();
            OnDialogueBack?.Invoke();
        }

        private void LateUpdate()
        {
            if (isTyping)
            {
                UpdateVertexVisibility();
            }
        }

        private void UpdateVertexVisibility()
        {
            if (field == null || field.textInfo == null || field.textInfo.characterCount == 0) return;

            if (currentVisibleCharacters >= field.textInfo.characterCount && !isTyping) return;

            TMP_TextInfo textInfo = field.textInfo;

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

            field.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}