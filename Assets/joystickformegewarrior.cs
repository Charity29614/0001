using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class joystickformegewarrior : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private FixedJoystick js;
    [SerializeField] private Animator ar;
    [SerializeField] float Speed = 5;
    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(js.Vertical, 0, -1 * js.Horizontal) * Speed;
        rb.linearVelocity = movement;

        // Determine animation based on Joystick movement
        if (js.Horizontal != 0 || js.Vertical != 0)
        {
            PlayAnimation("Run");
        }
        else
        {
            PlayAnimation("Idle");
        }

        if (movement != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(js.input.y, -1 * js.input.x) * Mathf.Rad2Deg;
            rb.rotation = Quaternion.Euler(0, targetAngle, 0); // Direct rotation without smoothing
        }

    }

    // Play animation without looping it
    public void PlayAnimation(string animationName)
    {
        AnimatorStateInfo currentState = ar.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName(animationName))
        {
            return; // Avoid restarting the same animation
        }

        ar.Play(animationName);
    }
}
