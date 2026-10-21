using TMPro;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [SerializeField] private float frequency = 3f;
    [SerializeField] private float amplitude = 5f;
    private Vector3 startPos;

	// Set position upon start
    private void Start()
    {
        startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the position on the sine wave according to the set amplitude and frequency
        float cycle = frequency * Time.time;
        float newCycle = Mathf.Sin(cycle);
        float newPositionY = amplitude * newCycle;
        
        // Assign the new position on the sine wave to the object
        Vector3 offset = new Vector3(0f, newPositionY, 0f);
        transform.localPosition = startPos + offset;
    }
}
