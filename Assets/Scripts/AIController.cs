using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField]
    Transform npcObject;
    private Vector3 currentDirection;
    public float directionChangeTimer;

    private bool isChilling = false;
    private float chillTimer;
    private float chillDuration;
    private int randomDirection = -1;

    Movement aiMovement;
    GravityControllerForMultipleFields gravityControllerForMultipleFields;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aiMovement = GetComponent<Movement>();
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
            if (chillTimer <= 0f)
            {
                isChilling = false;
                directionChangeTimer = Random.Range(2f, 8f); // Timer zurücksetzen
            }
        }
        else
        {
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0f)
            {
                directionChangeTimer = Random.Range(2f, 8f); // Timer zurücksetzen
                randomDirection = Random.Range(0, 4);
                StartChilling();
            }
            //gravityControllerForMultipleFields.RotateToPlanet();
            aiMovement.Move(currentDirection, npcObject, gravityDirection);
            gravityControllerForMultipleFields.RotateToPlanet();
        }
    }

    private void ChangeDirection(int randomDirection)
    {
        switch (randomDirection)
        {
            case 0:
                currentDirection = transform.forward;
                break;
            case 1:
                currentDirection = -transform.forward;
                break;
            case 2:
                currentDirection = transform.right;
                break;
            case 3:
                currentDirection = -transform.right;
                break;
        }
    }

    private void StartChilling()
    {
        isChilling = true;
        chillDuration = Random.Range(2f, 5f); // Zufällige Chill-Dauer
        chillTimer = chillDuration;
    }
}
