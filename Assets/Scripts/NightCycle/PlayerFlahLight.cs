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
        private bool can_shimmer = false;
        [SerializeField] float baseIntensity = 1.0f;

        [SerializeField] float swayAmount = 0.04f;
        [SerializeField] float swaySmooth = 8f;

        [SerializeField] Animator anim;
        [SerializeField] string trig_on;
        [SerializeField] string trig_off;

        public bool light_active = false;

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
            //Debug.Log(flashlight.intensity);
            if (!flashlight.enabled) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            //model.transform.localPosition = initialLocalPos;
            /*Vector3 targetPos = initialLocalPos +
                                new Vector3(-mouseX * swayAmount, -mouseY * swayAmount, 0f);

            transform.localPosition =
                Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * swaySmooth);*/

            //flashlight.intensity = baseIntensity + Mathf.Sin(Time.time * 2f) * 0.5f*baseIntensity;

            //if (can_shimmer)
            //{

            //flashlight.intensity = baseIntensity + Mathf.Sin(Time.time * 2f) * 0.5f * baseIntensity;
            //}

            if (Input.GetKeyDown(KeyCode.F))
            {
                //if (IsActiveLight())
                if (light_active)
                {

                    //TurnOFFLight();
                    //Debug.Log("dis");
                    play_disable();
                    //can_shimmer = false;
                    light_active = false;
                }
                else
                {
                    //TurnOnLight();
                    //Debug.Log("en");
                    play_enable();
                    //can_shimmer = true;
                    light_active = true;
                }
            }
        }

        private void play_enable()
        {
            anim.ResetTrigger(trig_off);
            anim.SetTrigger(trig_on);
        }

        private void play_disable()
        {
            anim.ResetTrigger(trig_on);
            anim.SetTrigger(trig_off);
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

        public void TurnOnLight()
        {
            flashlight.gameObject.SetActive(true);
        }

        public void TurnOFFLight()
        {
            flashlight.gameObject.SetActive(false);
        }

        public bool IsActiveLight()
        {
            return flashlight.gameObject.activeSelf;
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
