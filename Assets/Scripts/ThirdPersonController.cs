using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    //public Rigidbody rb;
    public Transform playerObject;
    public Transform cameraTransform;

    private Movement playerMovement;
    private GroundDetection groundDetection;
    private GravityController gravityController;
    private GravityControllerForMultipleFields gravityControllerForMultipleFields;

    [SerializeField]
    private bool isGrounded;

    [SerializeField]
    private bool useMultipleGravityFields = false;

    void Awake()
    {
        //rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<Movement>();
        groundDetection = GetComponent<GroundDetection>();
        gravityController = GetComponent<GravityController>();
        gravityControllerForMultipleFields = GetComponent<GravityControllerForMultipleFields>();
    }

    private void Update()
    {
        GroundCheck();
        ApplyGravityAndMovement();
    }

    private void ApplyGravityAndMovement()
    {
        if (useMultipleGravityFields)
        {
            gravityControllerForMultipleFields.ApplyGravitation();
            gravityControllerForMultipleFields.RotateToPlanet();
            Vector3 gravityDirection = gravityControllerForMultipleFields.GetGravityDirection();
            HandleMovementAndJump(gravityDirection);
        }
        // else
        // {
        //     gravityController.ApplyGravitation(rb);
        //     gravityController.RotateToPlanet(rb);
        //     Vector3 gravityDirection = gravityController.GetGravityDirection();
        //     HandleMovementAndJump(gravityDirection);
        // }
    }

    private void HandleMovementAndJump(Vector3 gravityDirection)
    {
        if (isGrounded)
        {
            playerMovement.Move(GetInputVector(), playerObject, gravityDirection, cameraTransform);
            playerMovement.Jump();
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

    public bool IsGrounded
    {
        get { return isGrounded; }
    }

    public Transform GetPlayerObject()
    {
        return playerObject;
    }
}
