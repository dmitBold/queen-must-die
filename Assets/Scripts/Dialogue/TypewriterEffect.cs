using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dialogue
{
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField] float delay = 0.03f;
        [SerializeField] int maxLines = 3;

        TextMeshProUGUI field;
        string fullText;
        int charIndex;
        int chunkIndex;

        Coroutine routine;
        bool isTyping;

        public List<string> chunks = new List<string>();

        public bool IsFinished => string.IsNullOrEmpty(fullText) || (chunkIndex == chunks.Count - 1 && charIndex >= fullText.Length);
        public bool CanGoBack => chunkIndex > 0;

        public event System.Action OnDialogueFinished;
        public event System.Action OnDialogueBack;

        public enum SkipResult
        {
            None,
            PageFinished,
            DialogueFinished
        }

        public void Play(TextMeshProUGUI text, string content)
        {
            if (routine != null) StopCoroutine(routine);

            field = text;
            fullText = content;
            charIndex = 0;
            chunkIndex = -1;
            chunks.Clear();

            ShowNextChunk();
        }

        void ShowNextChunk()
        {
            if (chunkIndex + 1 >= chunks.Count)
            {
                if (charIndex >= fullText.Length) return;
                chunks.Add(BuildChunk());
            }

            chunkIndex++;
            routine = StartCoroutine(Type(chunks[chunkIndex]));
        }

        string BuildChunk()
        {
            string remainingText = fullText.Substring(charIndex);
            field.text = remainingText;
            field.ForceMeshUpdate();

            if (field.textInfo.lineCount <= maxLines)
            {
                charIndex = fullText.Length;
                return remainingText;
            }

            int visibleOverflowIndex = field.textInfo.lineInfo[maxLines].firstCharacterIndex;
            int rawOverflowIndex = field.textInfo.characterInfo[visibleOverflowIndex].index;

            string chunk = remainingText.Substring(0, rawOverflowIndex);
            int consumedLength = rawOverflowIndex;

            if (chunk.EndsWith("\n"))
            {
                chunk = chunk.Substring(0, chunk.Length - 1) + " ";
            }
            else if (chunk.EndsWith("\r\n"))
            {
                chunk = chunk.Substring(0, chunk.Length - 2) + "  ";
            }

            charIndex += consumedLength;
            return chunk;
        }

        IEnumerator Type(string chunk)
        {
            isTyping = true;
            field.text = "";
            yield return new WaitForSeconds(delay);
            field.text = chunk;
            isTyping = false;

            if (IsFinished)
            {
                OnDialogueFinished?.Invoke();
            }
        }

        public SkipResult Skip()
        {
            if (string.IsNullOrEmpty(fullText)) return SkipResult.None;

            if (isTyping)
            {
                if (routine != null) StopCoroutine(routine);
                field.text = chunks[chunkIndex];
                isTyping = false;

                if (IsFinished)
                {
                    OnDialogueFinished?.Invoke();
                }

                return SkipResult.PageFinished;
            }

            if (!IsFinished)
            {
                ShowNextChunk();
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

            if (chunkIndex <= 0) return;

            chunkIndex--;
            field.text = chunks[chunkIndex];
            OnDialogueBack?.Invoke();
        }
    }
}