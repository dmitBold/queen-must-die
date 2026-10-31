using TMPro;
using UnityEngine;

namespace UI
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;

        public void Show(string content, Vector3 position)
        {
            //test
            if (this == null || gameObject == null)
            {
                UnityEngine.Debug.Log("BAD @ @ @");
                return;
            }
            UnityEngine.Debug.Log("GOOD @ @ @");
            //test
            text.text = content;
            transform.position = position;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            //test
            if (this == null) return;
            if (gameObject == null) return;
            //test

            gameObject.SetActive(false);
        }
    }
}