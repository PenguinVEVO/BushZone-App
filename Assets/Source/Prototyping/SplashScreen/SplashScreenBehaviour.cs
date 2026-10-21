using System.Collections;
using BZApp.GUI.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BZApp.Prototyping
{
    public class SplashScreenBehaviour : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad = "LoaderScene";
        [SerializeField] private float startDelayTime = 0.5f;
        [SerializeField] private float splashImageFadeTime = 0.5f;
        [SerializeField] private float sceneSwitchDelayTime = 0.1f;
        [SerializeField] private Image bzAppSplashImage;

        private AsyncOperation asyncLevelLoad;

        private void Start() => StartCoroutine(StartBehaviour());

        private IEnumerator StartBehaviour()
        {
            yield return new WaitForSeconds(startDelayTime);
            asyncLevelLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            asyncLevelLoad.allowSceneActivation = false;
            while (asyncLevelLoad.progress < 0.9f)
                yield return null;
            float timeElapsed = 0;
            while (timeElapsed < splashImageFadeTime)
            {
                timeElapsed += Time.deltaTime;
                bzAppSplashImage.color = Color.Lerp(Color.white, ExtColour.ClearWhite, timeElapsed / splashImageFadeTime);
                yield return null;
            }
            yield return new WaitForSeconds(sceneSwitchDelayTime);
            asyncLevelLoad.allowSceneActivation = true;
        }
    }
}
