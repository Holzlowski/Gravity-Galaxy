using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform playerObject;
    public Transform cameraTransform;

    private PlayerMovement playerMovement;
    private GroundDetection groundDetection;
    private GravityController gravityController;
    private GravityControllerForMultipleFields gravityControllerForMultipleFields;

    [SerializeField]
    private bool isGrounded;

    [SerializeField]
    private bool useMultipleGravityFields = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
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
            gravityControllerForMultipleFields.ApplyGravitation(rb);
            gravityControllerForMultipleFields.RotateToPlanet(rb);
            Vector3 gravityDirection = gravityControllerForMultipleFields.GetGravityDirection();
            HandleMovementAndJump(gravityDirection);
        }
        else
        {
            gravityController.ApplyGravitation(rb);
            gravityController.RotateToPlanet(rb);
            Vector3 gravityDirection = gravityController.GetGravityDirection();
            HandleMovementAndJump(gravityDirection);
        }
    }

    private void HandleMovementAndJump(Vector3 gravityDirection)
    {
        if (isGrounded)
        {
            playerMovement.Movement(rb, playerObject, cameraTransform, gravityDirection);
        }
        playerMovement.Jump(rb, isGrounded);
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
