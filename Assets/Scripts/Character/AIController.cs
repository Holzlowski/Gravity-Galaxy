using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField]
    Transform npcObject;
    private Vector3 currentDirection;
    private float directionChangeTimer;

    private bool isChilling = false;
    private float chillTimer;
    private float chillDuration;
    private int remainingChillJumps;
    private float jumpTimer;
    private int randomDirection = -1;

    Movement aiMovement;
    GravityControllerForMultipleFields gravityControllerForMultipleFields;
    GroundDetection groundDetection;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aiMovement = GetComponent<Movement>();
        groundDetection = GetComponent<GroundDetection>();
        gravityControllerForMultipleFields = GetComponent<GravityControllerForMultipleFields>();

        currentDirection = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 gravityDirection = gravityControllerForMultipleFields.GetGravityDirection();
        gravityControllerForMultipleFields.ApplyGravitation();
        ChangeDirection(randomDirection);

        if (isChilling)
        {
            chillTimer -= Time.deltaTime;
            jumpTimer -= Time.deltaTime; // Timer für den nächsten Sprung verringern

            if (chillTimer <= 0f)
            {
                isChilling = false;
                directionChangeTimer = Random.Range(2f, 8f); // Timer zurücksetzen
                remainingChillJumps = 0; // Sprünge zurücksetzen
            }

            // Wenn der NPC während des Chillens springen soll
            if (remainingChillJumps > 0 && jumpTimer <= 0f)
            {
                aiMovement.Jump(); // NPC springt
                remainingChillJumps--; // Anzahl der verbleibenden Chill-Sprünge verringern
                jumpTimer = Random.Range(0.5f, 1.5f); // Timer für den nächsten Sprung setzen
            }
        }
        else
        {
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                directionChangeTimer = Random.Range(2f, 8f); // Timer zurücksetzen
                randomDirection = Random.Range(0, 8);
                StartChilling();
            }
            else if (groundDetection.IsGrounded)
            {
                aiMovement.Move(currentDirection, npcObject, gravityDirection);
                aiMovement.SetLastMovementDirectionToZero();
            }
        }
        gravityControllerForMultipleFields.RotateToPlanet();
    }

    private void ChangeDirection(int randomDirection)
    {
        switch (randomDirection)
        {
            case 0:
                currentDirection = transform.forward; // Vorne
                break;
            case 1:
                currentDirection = -transform.forward; // Hinten
                break;
            case 2:
                currentDirection = transform.right; // Rechts
                break;
            case 3:
                currentDirection = -transform.right; // Links
                break;
            case 4:
                currentDirection = (transform.forward + transform.right).normalized; // Vorne-Rechts
                break;
            case 5:
                currentDirection = (transform.forward - transform.right).normalized; // Vorne-Links
                break;
            case 6:
                currentDirection = (-transform.forward + transform.right).normalized; // Hinten-Rechts
                break;
            case 7:
                currentDirection = (-transform.forward - transform.right).normalized; // Hinten-Links
                break;
        }
    }

    void StartChilling()
    {
        isChilling = true;
        chillTimer = Random.Range(1f, 3f); // Chillzeit festlegen
        remainingChillJumps = Random.Range(1, 3); // NPC springt während des Chillens 1 oder 2 Mal
        jumpTimer = Random.Range(0.5f, 1.5f); // Erster Sprung verzögert
    }
}
