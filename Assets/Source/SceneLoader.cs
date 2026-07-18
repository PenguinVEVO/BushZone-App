using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mitchel.Utilities
{
    public class SceneLoader : MonoBehaviour
    {
        private bool asyncSceneActivate = true;
        private float sceneLoaderProgress;
    
        #region SINGLETON INITIALISATION
        private static SceneLoader instance;
        private void Awake()
        {
            if (instance != null)
                Destroy(instance);
            instance = this;
            DontDestroyOnLoad(instance);
        }
        public static SceneLoader Instance => instance;
        #endregion

        /// <summary>
        /// Begins an AsyncOperation to load a new scene in the background.
        /// </summary>
        /// <param name="sceneName">The name of the new scene (must be an active build scene).</param>
        /// <param name="activateScene">Allow scene activation immediately. If false, ActivateQueuedScene() must be called manually.</param>
        public void BeginAsyncLoad(int sceneIndex, bool activateScene)
        {
            if (!activateScene) asyncSceneActivate = false;
            StartCoroutine(AsyncSceneLoad(sceneIndex));
        }

        private IEnumerator AsyncSceneLoad(int sceneIndex)
        {
            AsyncOperation sceneLoadAsync = SceneManager.LoadSceneAsync(sceneIndex);
            if (!asyncSceneActivate)
            {
                sceneLoadAsync.allowSceneActivation = false;
                while (!asyncSceneActivate)
                {
                    sceneLoaderProgress = sceneLoadAsync.progress;
                    Debug.Log(sceneLoaderProgress);
                    yield return null;
                }
                sceneLoadAsync.allowSceneActivation = true;
            }
        }

        public void ActivateQueuedScene()
        {
            asyncSceneActivate = true;
        }

        public float SceneLoaderProgress => sceneLoaderProgress;
    }
}