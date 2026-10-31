using UnityEngine;

namespace NightCycle
{
    public class AnimHelper : MonoBehaviour
    {
        public Animator animator;
        public string clip_name;

        public GameObject[] objects;

        void Animator_play()
        {
            animator.enabled = true;
            animator.Play(clip_name);
        }

        void change_rotation_Y(float rot)
        {
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.y = rot;
            transform.eulerAngles = currentEuler;
        }

        public void TurnOffTarget(int index)
        {
            objects[index].SetActive(false);
        }

        public void TurnOnTarget(int index)
        {
            objects[index].SetActive(true);
        }

        public void Enable_Cam(Camera cam)
        {
            cam.enabled = true;
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }
    }
}
