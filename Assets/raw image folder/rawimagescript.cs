using UnityEngine;

public class rawimagescript : MonoBehaviour
{
    // Reference to the RawImage component
    private UnityEngine.UI.RawImage rawImage;

    // Speed at which the UV Rect moves (0.01 per second for both X and Y)
    public float moveSpeed = 0.01f;

    private void Start()
    {
        // Get the RawImage component attached to the GameObject
        rawImage = GetComponent<UnityEngine.UI.RawImage>();
    }

    private void Update()
    {
        // Modify the UV Rect (the portion of the texture shown)
        Rect uvRect = rawImage.uvRect;

        // Move the UV Rect by 0.01 units per second for both X and Y
        uvRect.x += moveSpeed * Time.deltaTime;
        uvRect.y += moveSpeed * Time.deltaTime;

        // Apply the modified UV Rect back to the RawImage
        rawImage.uvRect = uvRect;
    }
}