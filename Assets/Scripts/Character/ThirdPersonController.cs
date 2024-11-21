using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform playerObject;

    [SerializeField]
    private Transform cameraTransform;

    private Movement playerMovement;
    private GroundDetection groundDetection;
    private GravityControllerForMultipleFields gravityControllerForMultipleFields;

    [SerializeField]
    private bool isGrounded;

    void Awake()
    {
        //rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<Movement>();
        groundDetection = GetComponent<GroundDetection>();
        gravityControllerForMultipleFields = GetComponent<GravityControllerForMultipleFields>();
    }

    private void Update()
    {
        GroundCheck();
        ApplyGravityAndMovement();
    }

    private void ApplyGravityAndMovement()
    {
        Vector3 gravityDirection = gravityControllerForMultipleFields.GetGravityDirection();
        HandleMovementAndJump(gravityDirection);
        gravityControllerForMultipleFields.ApplyGravitation();
        gravityControllerForMultipleFields.RotateToPlanet();
    }

    private void HandleMovementAndJump(Vector3 gravityDirection)
    {
        if (isGrounded)
        {
            playerMovement.Move(GetInputVector(), playerObject, gravityDirection, cameraTransform);
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

    // public bool IsGrounded
    // {
    //     get { return isGrounded; }
    // }
}
