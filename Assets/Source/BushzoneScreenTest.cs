using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BushzoneScreenTest : MonoBehaviour
{
    [Header("Spinning Globe")]
    [SerializeField] private GameObject globe;
    [SerializeField] private float spinSpeed;
    
    [Header("Connecting Text")]
    [SerializeField] private Image connectingText;
    [SerializeField] private float fadeTime;
    private bool fadeIn = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DoGlobeSpin());
        StartCoroutine(DoFlashingText());
    }

    private IEnumerator DoGlobeSpin()
    {
        float rotation = spinSpeed * Time.deltaTime;
        while (true)
        {
            globe.transform.Rotate(0, rotation, 0, Space.Self);
            yield return null;
        }
    }

    private IEnumerator DoFlashingText()
    {
        Color opaque = new Color(connectingText.color.r, connectingText.color.g, connectingText.color.b, 1);
        Color transparent = new Color(connectingText.color.r, connectingText.color.g, connectingText.color.b, 0);
        float timeElapsed = 0;
        
        while (true)
        {
            if (fadeIn)
            {
                connectingText.color = Color.Lerp(transparent, opaque, timeElapsed / fadeTime);
                if (timeElapsed >= fadeTime)
                {
                    fadeIn = false;
                    timeElapsed = 0;
                }
                
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            else
            {
                connectingText.color = Color.Lerp(opaque, transparent, timeElapsed / fadeTime);
                if (timeElapsed >= fadeTime)
                {
                    fadeIn = true;
                    timeElapsed = 0;
                }
                
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
