using Dialogue;
using UnityEngine;

namespace NightCycle
{
    public class ThoughtController : MonoBehaviour
    {
        public static ThoughtController Instance;

        [SerializeField] ThoughtView view;

        private bool isActive = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            view.Hide();
        }

        public void ShowThought(string text)
        {
            if (isActive) return;

            isActive = true;

            PlayerStateController.Instance.SetMode(PlayerStateController.PlayerMode.Focused);

            view.Show();
            view.PlayText(text);

            HUDController.instance.DisableInteractionText();
        }

        public void CloseThought()
        {
            if (!isActive) return;

            isActive = false;

            view.Hide();

            PlayerStateController.Instance.SetMode(PlayerStateController.PlayerMode.FreeMovement);
        }

        private void Update()
        {
            if (!isActive) return;


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseThought();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnNextPressed();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnBackPressed();
            }
        }

        public void OnNextPressed()
        {
            var result = view.Skip();

            if (result == TypewriterEffect.SkipResult.DialogueFinished)
            {
                CloseThought();
            }
        }

        public void OnBackPressed()
        {
            view.Back();
        }
    }
}