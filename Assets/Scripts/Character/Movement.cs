using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField]
    private float baseJumpForce = 10f;

    [SerializeField]
    private float additionalJumpForce = 0.5f;

    [SerializeField]
    private int maxAdditionalHigherJumps = 2;

    [SerializeField]
    private float jumpCooldown = 0.5f;

    private Vector3 lastMovementDirection;
    private int jumpCount = 0;
    private float lastJumpTime = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(
        Vector3 directionVector,
        Transform characterObject,
        Transform cameraTransform = null // Optionales cameraTransform
    )
    {
        Vector3 movementDirection = CalculateMovementDirection(directionVector, cameraTransform);

        if (movementDirection.magnitude >= 0.1f)
        {
            lastMovementDirection = movementDirection; // Letzte Bewegungsrichtung speichern
            RotateTowardsMoveDirection(characterObject, movementDirection);
            rb.MovePosition(rb.position + movementDirection * speed * Time.deltaTime);
        }
        else if (lastMovementDirection != Vector3.zero)
        {
            lastMovementDirection = Vector3.zero; // Zurücksetzen nur, wenn vorher ein Wert vorhanden war
        }
    }

    public void Jump()
    {
        // Überprüfung, ob der letzte Sprung innerhalb des Zeitfensters war 
        // und ob die maximale Anzahl an Sprüngen noch nicht erreicht wurde
        if (CanJumpHigher())
        {
            jumpCount++;
        }
        else
        {
            jumpCount = 0;
        }
        lastJumpTime = Time.time;

        Vector3 jumpDirection = CalculateJumpDirection();

        rb.AddForce(jumpDirection, ForceMode.Impulse);
    }

    private Vector3 CalculateMovementDirection(Vector3 directionVector, Transform cameraTransform)
    {
        Vector3 movementDirection;
        if (cameraTransform != null)
        {
            // Spielerbewegung relativ zur Kamera
            Vector3 cameraRotation = new Vector3(0, cameraTransform.localEulerAngles.y, 0);
            Vector3 rotatedDirection = Quaternion.Euler(cameraRotation) * directionVector;
            movementDirection =
                transform.forward * rotatedDirection.z + transform.right * rotatedDirection.x;
        }
        else
        {
            // NPC-Bewegung
            movementDirection = directionVector;
        }

        return movementDirection;
    }

    private void RotateTowardsMoveDirection(
        Transform characterObject,
        Vector3 movementDirection
    )
    {
        Quaternion targetRotation = Quaternion.LookRotation(movementDirection, transform.up);
        characterObject.rotation = Quaternion.Slerp(
            characterObject.rotation,
            targetRotation,
            Time.fixedDeltaTime * rotationSpeed
        );
    }

    private bool CanJumpHigher()
    {
        return Time.time - lastJumpTime <= jumpCooldown && jumpCount < maxAdditionalHigherJumps;
    }

    private Vector3 CalculateJumpDirection()
    {
        // Sprungkraft basierend auf der Anzahl der aufeinanderfolgenden Sprünge
        float currentJumpForce = baseJumpForce * (1 + additionalJumpForce * jumpCount);

        Vector3 jumpDirection = transform.up * currentJumpForce * 0.5f;
        if (lastMovementDirection.magnitude > 0.1f)
        {
            jumpDirection += lastMovementDirection * currentJumpForce; // Letzte Bewegungsrichtung hinzu
        }
        return jumpDirection;
    }

    public void SetLastMovementDirectionToZero()
    {
        lastMovementDirection = Vector3.zero;
    }
}
