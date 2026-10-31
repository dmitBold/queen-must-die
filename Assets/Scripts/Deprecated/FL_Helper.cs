using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class FL_Helper : MonoBehaviour
    {
        [Inject] PlayerFlashlight flashlight;
        public void Start_Anim()
        {
            flashlight.flashlight.GetComponent<Animator>().enabled = true;
        }

        public void Disable_Light()
        {
            flashlight.flashlight.gameObject.SetActive(false);
        }

        /*public void ttest()
        {
            flashlight.test();
        }*/


    }
}
