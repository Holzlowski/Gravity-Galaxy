using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 10f;
    public float rotationSpeed = 10f;
    public float jumpForce = 10f;
    private Vector3 lastMovementDirection;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Movement(
        Rigidbody rb,
        Transform playerObject,
        Transform cameraTransform,
        Vector3 gravityDirection
    )
    {
        Vector3 inputVector = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")
        );
        Vector3 cameraRotation = new Vector3(0, cameraTransform.localEulerAngles.y, 0);
        Vector3 direction = Quaternion.Euler(cameraRotation) * inputVector;
        Vector3 movement_dir = transform.forward * direction.z + transform.right * direction.x;

        if (inputVector.magnitude >= 0.1f)
        {
            lastMovementDirection = movement_dir; // Letzte Bewegungsrichtung

            Quaternion targetRotation = Quaternion.LookRotation(movement_dir, -gravityDirection);
            playerObject.rotation = Quaternion.Slerp(
                playerObject.rotation,
                targetRotation,
                Time.fixedDeltaTime * rotationSpeed
            );

            rb.MovePosition(rb.position + movement_dir * speed * Time.fixedDeltaTime);
        }
        else
        {
            lastMovementDirection = Vector3.zero;
        }
    }

    public void Jump(Rigidbody rb, bool isGrounded)
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector3 jumpDirection = transform.up * jumpForce * 0.5f; // Sprungrichtung

            if (lastMovementDirection.magnitude > 0.1f)
            {
                jumpDirection += lastMovementDirection * jumpForce; // Füge die letzte Bewegungsrichtung hinzu
            }

            rb.AddForce(jumpDirection, ForceMode.Impulse);
        }
    }
}
