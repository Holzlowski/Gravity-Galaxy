using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody rb;
    public float speed = 10f;
    public float rotationSpeed = 10f;
    public float baseJumpForce = 10f;
    private Vector3 lastMovementDirection;

    //private float currentGravityStrength = 9.81f;

    private int jumpCount = 0;
    private int maxJumpCount = 2;
    private float lastJumpTime = 0;
    public float jumpCooldown = 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Move(
        Vector3 directionVector,
        Transform playerObject,
        Vector3 gravityDirection,
        Transform cameraTransform = null // Optionales cameraTransform
    )
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

        if (movementDirection.magnitude >= 0.1f)
        {
            lastMovementDirection = movementDirection; // Letzte Bewegungsrichtung speichern

            Quaternion targetRotation = Quaternion.LookRotation(
                movementDirection,
                -gravityDirection
            );
            playerObject.rotation = Quaternion.Slerp(
                playerObject.rotation,
                targetRotation,
                Time.fixedDeltaTime * rotationSpeed
            );

            rb.MovePosition(rb.position + movementDirection * speed * Time.deltaTime);
        }
        else
        {
            lastMovementDirection = Vector3.zero;
        }
    }

    public void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Überprüfung, ob der letzte Sprung innerhalb des Zeitfensters war
            if (Time.time - lastJumpTime <= jumpCooldown && jumpCount <= maxJumpCount)
            {
                jumpCount++;
            }
            else
            {
                jumpCount = 1;
            }

            lastJumpTime = Time.time;

            float currentJumpForce;

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

            // Sprungkraft basierend auf der Anzahl der aufeinanderfolgenden Sprünge
            currentJumpForce = baseJumpForce * (1 + 0.5f * (jumpCount - 1));

            Vector3 jumpDirection = transform.up * currentJumpForce * 0.5f; // Sprungrichtung

            if (lastMovementDirection.magnitude > 0.1f)
            {
                jumpDirection += lastMovementDirection * currentJumpForce; // Letzte Bewegungsrichtung hinzu
            }

            rb.AddForce(jumpDirection, ForceMode.Impulse);
        }
    }

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
