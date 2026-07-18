using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VersionText : MonoBehaviour
{
    [SerializeField] private string versionPrefix;
    private TextMeshProUGUI versionText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        versionText = GetComponent<TextMeshProUGUI>();
        versionText.text = $"{versionPrefix} v{Application.version}";
    }
}