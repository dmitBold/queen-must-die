using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Общий сервис для загрузки (аддитивно или полностью) и выгрузки сцен.
    /// Требует MonoBehaviour-раннер для запуска корутин.
    /// </summary>
    public class ScenesManager
    {
        private readonly MonoBehaviour _runner;

        public ScenesManager(MonoBehaviour runner)
        {
            _runner = runner;
        }

        public void LoadAdditive(string sceneName, Action<Scene> onLoaded = null)
        {
            _runner.StartCoroutine(LoadAdditiveRoutine(sceneName, onLoaded));
        }

        public void LoadSingle(string sceneName, Action onLoaded = null)
        {
            _runner.StartCoroutine(LoadSingleRoutine(sceneName, onLoaded));
        }

        public void Unload(string sceneName, Action onUnloaded = null)
        {
            _runner.StartCoroutine(UnloadRoutine(sceneName, onUnloaded));
        }

        public void SetActive(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
                SceneManager.SetActiveScene(scene);
            else
                Debug.LogError($"[ScenesManager] Scene '{sceneName}' is not loaded.");
        }

        private IEnumerator LoadAdditiveRoutine(string sceneName, Action<Scene> onLoaded)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                onLoaded?.Invoke(SceneManager.GetSceneByName(sceneName));
                yield break;
            }

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene loaded = SceneManager.GetSceneByName(sceneName);
            onLoaded?.Invoke(loaded);
        }

        private IEnumerator LoadSingleRoutine(string sceneName, Action onLoaded)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            onLoaded?.Invoke();
        }

        private IEnumerator UnloadRoutine(string sceneName, Action onUnloaded)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
                yield break;

            yield return SceneManager.UnloadSceneAsync(scene);
            onUnloaded?.Invoke();
        }
    }
}
