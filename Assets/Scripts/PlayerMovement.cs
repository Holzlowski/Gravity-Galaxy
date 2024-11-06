using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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

            rb.MovePosition(rb.position + movement_dir * speed * Time.deltaTime);
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
