using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PlayerFlashlight : MonoBehaviour
    {

        //test
        //[Inject] private SaveSystem _saveSystem;
        //test
        //[SerializeField] Light flashlight;
        public Light flashlight;
        [SerializeField] float baseIntensity = 1.0f;

        [SerializeField] float swayAmount = 0.04f;
        [SerializeField] float swaySmooth = 8f;

        Vector3 initialLocalPos;


        void Awake()
        {
            initialLocalPos = transform.localPosition;
            baseIntensity = flashlight.intensity;
            /*if (_saveSystem.HasLoadedData)
            {
                var data = _saveSystem.CurrentData;
                if (data.isLightOn)
                {
                    TurnOn();
                }
                TurnOFF();
            }*/
        }
        
        private void Update()
        {
            if (!flashlight.enabled) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            //model.transform.localPosition = initialLocalPos;
            Vector3 targetPos = initialLocalPos +
                                new Vector3(-mouseX * swayAmount, -mouseY * swayAmount, 0f);

            transform.localPosition =
                Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * swaySmooth);

            flashlight.intensity = baseIntensity + Mathf.Sin(Time.time * 2f) * 0.5f*baseIntensity;
        }

        public void TurnOn()
        {
            this.gameObject.SetActive(true);
        }

        public void TurnOFF()
        {
            this.gameObject.SetActive(false);
        }

        public bool IsActive()
        {
            return this.gameObject.activeSelf;
        }

        public void StatueChase()
        {
            StatueController[] Statues = Object.FindObjectsByType<StatueController>(FindObjectsSortMode.None);

            foreach (StatueController statue in Statues)
            {
                statue.advance_pose();
            }

        }

    }
}
