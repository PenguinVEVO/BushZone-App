using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlowState.DataParser
{
    public class LoaderSceneDataParser : MonoBehaviour
    {
        public Image whiteImage;
        public GameObject loadingStatusTextGroup;
        public TextMeshProUGUI dateAndTimeText;
        [HideInInspector] public AsyncOperation sceneLoader;
        
        #region =====  SINGLETON INITIALISATION =====
        public static LoaderSceneDataParser Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;
        }
        #endregion
    }
}
