using UnityEngine;

public class FixCameraRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // Store the initial rotation of the camera
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // Lock the camera's rotation to the initial rotation
        transform.rotation = initialRotation;
    }
}
