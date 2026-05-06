/*using Dialogue;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class ThoughtController : MonoBehaviour
    {
        public static ThoughtController Instance;

        [SerializeField] ThoughtView view;

        private bool isActive = false;
        private PlayerStateController playerStateController;

        [Inject]
        private void Construct(PlayerStateController playerStateController)
        {
            this.playerStateController = playerStateController;
        }

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

            playerStateController.SetMode(PlayerMode.Focused);

            view.Show();
            view.PlayText(text);

            HUDController.instance.DisableInteractionText();
        }

        public void CloseThought()
        {
            if (!isActive) return;

            isActive = false;

            view.Hide();

            playerStateController.SetMode(PlayerMode.FreeMovement);
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
}*/