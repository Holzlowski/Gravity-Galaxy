using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    [SerializeField]
    private Transform groundCheck;

    [SerializeField]
    private float groundRadius = 0.2f;

    [SerializeField]
    private LayerMask whatIsGround;
    private bool isGrounded = false;

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    private void Update()
    {
        CheckGroundStatus();
    }

    private void CheckGroundStatus()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, whatIsGround);
    }
}
