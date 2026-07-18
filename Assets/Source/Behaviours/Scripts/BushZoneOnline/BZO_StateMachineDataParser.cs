using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mitchel.DataParser
{
    public class BZO_StateMachineDataParser : MonoBehaviour
    {
        public Image fadeImage;
        public TextMeshProUGUI loadingText;
    
        #region SINGLETON INITIALISATION
        private static BZO_StateMachineDataParser instance;
        private void Awake()
        {
            if (instance != null)
                Destroy(instance);
            instance = this;
        }
        public static BZO_StateMachineDataParser Instance => instance;
        #endregion
    }
}