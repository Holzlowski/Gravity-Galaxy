using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask whatIsGround;
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
