using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class RaycastDragon : MonoBehaviour
{
    // References to the leg transforms
    public Transform frontLeftLeg;
    public Transform frontRightLeg;
    public Transform backLeftLeg;
    public Transform backRightLeg;

    // LayerMask for the ground (to ensure we only raycast against terrain)
    public LayerMask groundLayer;

    public bool ShowPhysicsRaycastDrawing = false;
    // Offset to keep the dragon slightly above the ground
    public float groundOffset = 0.01f;  // Minimal distance from the ground

    // Maximum distance for raycasts
    public float raycastDistance = 2f;  // Shorter raycast distance to hit the cube surface

    // Reference to the Rigidbody
    private Rigidbody rb;

    private void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        AlignToGround();
    }

    private void AlignToGround()
    {
        // Perform raycasts from each leg and get the height at each leg's position
        float frontLeftHeight = GetGroundHeight(frontLeftLeg.position);
        float frontRightHeight = GetGroundHeight(frontRightLeg.position);
        float backLeftHeight = GetGroundHeight(backLeftLeg.position);
        float backRightHeight = GetGroundHeight(backRightLeg.position);

        // Find the lowest ground point among the legs
        float lowestPoint = Mathf.Min(frontLeftHeight, frontRightHeight, backLeftHeight, backRightHeight);

        // Adjust the dragon's position using the Rigidbody, so it stays above the ground
        Vector3 currentPosition = rb.position;
        currentPosition.y = lowestPoint + groundOffset; // Add a small offset to keep above the ground
        rb.MovePosition(currentPosition); // Use Rigidbody to move the dragon
    }

    private float GetGroundHeight(Vector3 origin)
    {
        // Perform a raycast downward from the leg
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            // Visualize the raycast for debugging purposes in the Scene view
            if (ShowPhysicsRaycastDrawing)
            Debug.DrawRay(origin, Vector3.down * raycastDistance, Color.red);

            // Return the height of the ground where the ray hit
            return hit.point.y;
        }

        // If no ground was hit, keep the dragon at the same height
        return transform.position.y;
    }
}
