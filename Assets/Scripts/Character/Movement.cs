using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float rotationSpeed = 10f;

    [SerializeField]
    private float baseJumpForce = 10f;

    [SerializeField]
    private Vector3 lastMovementDirection;

    //private float currentGravityStrength = 9.81f;

    private int jumpCount = 0;

    [SerializeField]
    private int maxJumpCount = 2;
    private float lastJumpTime = 0;

    [SerializeField]
    private float jumpCooldown = 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(
        Vector3 directionVector,
        Transform characterObject,
        Vector3 gravityDirection,
        Transform cameraTransform = null // Optionales cameraTransform
    )
    {
        Vector3 movementDirection = CalculateMovementDirection(directionVector, cameraTransform);

        if (movementDirection.magnitude >= 0.1f)
        {
            lastMovementDirection = movementDirection; // Letzte Bewegungsrichtung speichern
            RotateTowardsMoveDirection(characterObject, gravityDirection, movementDirection);
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
        if (CanJumpAgain())
        {
            jumpCount++;
        }
        else
        {
            jumpCount = 1;
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
            // Spielerbewegung: Nutze Kameraausrichtung
            Vector3 cameraRotation = new Vector3(0, cameraTransform.localEulerAngles.y, 0);
            Vector3 rotatedDirection = Quaternion.Euler(cameraRotation) * directionVector;
            movementDirection =
                transform.forward * rotatedDirection.z + transform.right * rotatedDirection.x;
        }
        else
        {
            // NPC-Bewegung: Nutze directionVector direkt
            movementDirection = directionVector;
        }

        return movementDirection;
    }

    private void RotateTowardsMoveDirection(
        Transform characterObject,
        Vector3 gravityDirection,
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

    private bool CanJumpAgain()
    {
        return Time.time - lastJumpTime <= jumpCooldown && jumpCount <= maxJumpCount;
    }

    private Vector3 CalculateJumpDirection()
    {
        // Sprungkraft basierend auf der Anzahl der aufeinanderfolgenden Sprünge
        float currentJumpForce = baseJumpForce * (1 + 0.5f * (jumpCount - 1));

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

    // if (GameManager.Instance.equalJumpHeight)
    // {
    //     float gravityStrength = GetGravityStrengthFromGround();
    //     // Berechnung der Sprungkraft basierend auf der Gravitationsstärke und der Anzahl der Sprünge
    //     // Die Formel basiert auf der Gleichung für die kinetische Energie und die potentielle Energie:
    //     // E_kin = 1/2 * m * v^2 und E_pot = m * g * h
    //     // Um die Höhe h zu erreichen, muss die kinetische Energie in potentielle Energie umgewandelt werden:
    //     // 1/2 * m * v^2 = m * g * h => v = sqrt(2 * g * h)
    //     // Die Sprungkraft F ist proportional zur Geschwindigkeit v:
    //     // F = sqrt(2 * g * h) * (1 + 0.5 * (jumpCount - 1))
    //     currentJumpForce =
    //         Mathf.Sqrt(2 * gravityStrength * baseJumpForce) * (1 + 0.5f * (jumpCount - 1));
    // }

    // private float GetGravityStrengthFromGround()
    // {
    //     RaycastHit hit;
    //     if (Physics.Raycast(transform.position, -transform.up, out hit, 2.5f))
    //     {
    //         int groundLayer = LayerMask.NameToLayer("Ground");
    //         if (hit.collider.gameObject.layer == groundLayer)
    //         {
    //             GravityField gravityField = hit.collider.GetComponentInParent<GravityField>();
    //             if (gravityField != null)
    //             {
    //                 float newGravityStrength = gravityField.GetGravityStrength();
    //                 if (newGravityStrength != currentGravityStrength)
    //                 {
    //                     currentGravityStrength = newGravityStrength;
    //                     Debug.Log("Neue Gravitationsstärke: " + currentGravityStrength);
    //                 }
    //             }
    //         }
    //     }
    //     return currentGravityStrength;
    // }
}
