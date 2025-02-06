using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform playerCharacter;

    [SerializeField]
    private Transform cameraTransform;

    private Movement playerMovement;
    private GroundDetection groundDetection;
    private GravityController gravityController;
    private Animator animator;

    [SerializeField]
    private bool isGrounded;

    void Awake()
    {
        playerMovement = GetComponent<Movement>();
        groundDetection = GetComponent<GroundDetection>();
        gravityController = GetComponent<GravityController>();
        animator = playerCharacter.GetComponent<Animator>();
    }

    private void Update()
    {
        GroundCheck();
        ApplyGravityAndMovement();
        animator.SetBool("Grounded", isGrounded);
    }

    private void ApplyGravityAndMovement()
    {
        HandleMovementAndJump();
        gravityController.ApplyGravitation();
        gravityController.RotateToPlanet();
    }

    private void HandleMovementAndJump()
    {
        if (isGrounded)
        {
            playerMovement.Move(GetInputVector(), playerCharacter, cameraTransform);
            animator.SetFloat("MoveSpeed", GetInputVector().magnitude);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerMovement.Jump();
            }
        }
    }

    private static Vector3 GetInputVector()
    {
        return new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    private void GroundCheck()
    {
        isGrounded = groundDetection.IsGrounded;
    }
}
