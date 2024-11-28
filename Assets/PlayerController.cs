using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody _rigidBody;

    private Camera _mainCamera;
    private Quaternion fixedCameraRotation;
    [SerializeField] float Speed = 10;
    private void Start()
    {
        _mainCamera = Camera.main;
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();

        if (_mainCamera != null )
            fixedCameraRotation = _mainCamera.transform.rotation;

        GameObject PlayerObject = FindMultiTag.FindMultiTagByTag("Player");
        if (PlayerObject != null) Debug.Log($"{PlayerObject.name}");
    }

    private void Update()
    {
        // Get raw horizontal and vertical input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Determine animation based on movement
        if (horizontal != 0 || vertical != 0)
        {
            PlayAnimation("Run");
        }
        else
        {
            PlayAnimation("Idle");
        }

        // Calculate movement vector
        Vector3 movement = new Vector3(vertical, 0, -1 * horizontal).normalized * Speed;

        // Update linear velocity
        Vector3 currentVelocity = _rigidBody.linearVelocity;
        _rigidBody.linearVelocity = new Vector3(movement.x, currentVelocity.y, movement.z);

        // Keep the camera's rotation fixed
        if (_mainCamera != null)
        {
            _mainCamera.transform.rotation = fixedCameraRotation;
        }
    }

    // Play animation without looping it
    public void PlayAnimation(string animationName)
    {
        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName(animationName))
        {
            return; // Avoid restarting the same animation
        }

        _animator.Play(animationName);
    }
}
