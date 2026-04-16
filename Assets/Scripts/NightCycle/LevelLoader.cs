using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightCycle
{
    public class LevelLoader : MonoBehaviour
    {

        public Animator transition;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                LoadNext();
            }
        }

        public void LoadNext(){
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
        }

        IEnumerator LoadLevel(int LevelIndex)
        {
            transition.SetTrigger("Start");

            yield return new WaitForSeconds(2);

            SceneManager.LoadScene(LevelIndex);

        }

        //TEST
        private void Awake()
        {
            Application.targetFrameRate = 60;
        }
        //TEST

    }
}
