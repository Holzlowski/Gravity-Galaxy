using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] Transform npcObject;
    [SerializeField] Transform player; // Spieler-Referenz hinzufügen
    [SerializeField] Transform eyeObject;

    private Vector3 currentDirection;
    private float directionChangeTimer;

    private bool isRelaxing = false;
    private float relaxTimer;
    private bool hasJumped = false; // Kontrolliert, ob der NPC im aktuellen Relaxing gesprungen ist
    private int randomDirection = -1;

    [Range(0f, 1f)]
    [SerializeField] private float jumpChance = 0.5f; // Wahrscheinlichkeit, dass der NPC springt

    public float followRange = 10f;       // Reichweite, in der der NPC den Spieler verfolgt
    public float fieldOfView = 120f;        // Sichtfeld des NPCs in Grad (optional)

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

        // Spieler-Verfolgungslogik:
        if (player != null)
        {
            // Berechne den Ursprung des Raycasts in Augenhöhe (falls eyeObject zugewiesen ist)
            Vector3 eyePosition = eyeObject != null ? eyeObject.position : transform.position;
            // Berechne die Richtung vom Ursprung des Raycasts zum Spieler
            Vector3 directionToPlayer = (player.position - eyePosition).normalized;
            // Berechne den Abstand vom Augen-Objekt zum Spieler
            float distanceToPlayer = Vector3.Distance(eyePosition, player.position);

            if (distanceToPlayer < followRange)
            {
                // Überprüfe, ob der Spieler im Sichtfeld ist
                if (Vector3.Angle(transform.forward, directionToPlayer) < fieldOfView / 2f)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(eyePosition, directionToPlayer, out hit, followRange))
                    {
                        // Zeichne den Raycast grün, wenn der Spieler getroffen wird, sonst rot
                        if (hit.transform == player)
                        {
                            //Debug.DrawRay(eyePosition, directionToPlayer * followRange, Color.green);
                            // Spieler in Reichweite und freier Sichtlinie: Spieler verfolgen
                            currentDirection = directionToPlayer;
                            aiMovement.Move(currentDirection, npcObject);
                            gravityController.RotateToPlanet();
                            return; // Frühzeitiger Rücksprung, damit der Random-Wander-Code nicht ausgeführt wird.
                        }
                        else
                        {
                            //Debug.DrawRay(eyePosition, directionToPlayer * followRange, Color.red);
                        }
                    }
                }
            }
        }

        // Bestehende Random-Wanderlogik:
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
