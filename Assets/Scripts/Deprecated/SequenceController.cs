using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace NightCycle
{
    [RequireComponent(typeof(PlayableDirector))]
    public class SequenceController : MonoBehaviour
    {
        private PlayableDirector director;

        [Header("������� ��������")]
        public UnityEvent onCutsceneStart;
        public UnityEvent onCutsceneEnd;

        [Header("������")]
        public Camera CinemaCamera;

        public Vector3 pos;
        public Vector3 player_pos;

        void OnEnable()
        {
            director = GetComponent<PlayableDirector>();

            if (director != null)
            {
                director.played += OnPlayableDirectorPlayed;
                director.stopped += OnPlayableDirectorStopped;
            }
            else
            {
                Debug.LogError("PlayableDirector �� ������ �� �������!");
            }
        }

        void OnDisable()
        {
            if (director != null)
            {
                director.played -= OnPlayableDirectorPlayed;
                director.stopped -= OnPlayableDirectorStopped;
            }
        }

        private void OnPlayableDirectorPlayed(PlayableDirector pd)
        {
            Debug.Log("�������� ��������!");

            if (onCutsceneStart != null)
                onCutsceneStart.Invoke();
        }

        private void OnPlayableDirectorStopped(PlayableDirector pd)
        {
            Debug.Log("�������� �����������!");

            if (onCutsceneEnd != null)
                onCutsceneEnd.Invoke();
        }

        public void CameraON()
        {
            Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");   
            if (CinemaCamera != null)
            {
                CinemaCamera.enabled = true;
            }
            else
            {
                Debug.LogWarning("CinemaCamera �� ���������!");
            }
        }

        public void CameraOFF()
        {
            Debug.Log("&&&&&&&&&&&&&&&&&&&&&&&&&&");
            if (CinemaCamera != null)
            {
                CinemaCamera.enabled = false;
            }
        }

        // ===== ���������� ������� =====

        public void SceneON(GameObject scene)
        {
            if (scene != null)
            {
                scene.SetActive(true);
            }

            gameObject.SetActive(false);
        }

        public void OFF()
        {
            gameObject.SetActive(false);
        }

        void Start()
        {
            if (director != null && director.state == PlayState.Playing)
            {
                OnPlayableDirectorPlayed(director);
            }
        }

        public void ObjOFF(GameObject obj)
        {
            obj.SetActive(false);
        }

        public void ObjON(GameObject obj)
        {
            obj.SetActive(true);
        }


        public void Setpos(Vector3 vec)
        {
            pos = vec;
        }

        public void MoveObj(GameObject obj)
        {
            obj.transform.position = pos;
        }

        public void MovePL(GameObject obj)
        {
            obj.transform.position = player_pos;
        }

        public void C_OFF(Camera cam)
        {
            cam.enabled = false;    
        }

        public void C_ON(Camera cam)
        {
            cam.enabled = true;
        }

    }
}