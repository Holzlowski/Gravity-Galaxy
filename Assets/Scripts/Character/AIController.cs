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

    private bool isRelaxing = false;
    private float relaxTimer;
    private bool hasJumped = false; // Kontrolliert, ob der NPC im aktuellen Relaxing gesprungen ist
    private int randomDirection = -1;

    [Range(0f, 1f)]
    [SerializeField]
    private float jumpChance = 0.5f; // Wahrscheinlichkeit, dass der NPC springt

    Movement aiMovement;
    GravityController gravityController;
    GroundDetection groundDetection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aiMovement = GetComponent<Movement>();
        groundDetection = GetComponent<GroundDetection>();
        gravityController = GetComponent<GravityController>();

        currentDirection = transform.forward;
    }

    void Update()
    {
        gravityController.ApplyGravitation();
        ChangeDirection(randomDirection);

        if (isRelaxing)
        {
            relaxTimer -= Time.deltaTime;

            if (relaxTimer <= 0f)
            {
                isRelaxing = false;
                hasJumped = false;
                directionChangeTimer = Random.Range(2f, 8f);
            }

            if (!hasJumped && groundDetection.IsGrounded && Random.value < jumpChance)
            {
                aiMovement.Jump();
                hasJumped = true;
            }
        }
        else
        {
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                directionChangeTimer = Random.Range(2f, 8f);
                randomDirection = Random.Range(0, 8);
                StartRelaxing();
            }
            else if (groundDetection.IsGrounded)
            {
                aiMovement.Move(currentDirection, npcObject);
                aiMovement.SetLastMovementDirectionToZero();
            }
        }
        gravityController.RotateToPlanet();
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

    void StartRelaxing()
    {
        isRelaxing = true;
        relaxTimer = Random.Range(1f, 3f); // Relaxzeit festlegen
        hasJumped = false; // Sicherstellen, dass der NPC in jedem Relax-Zyklus einmal springen kann
    }
}
