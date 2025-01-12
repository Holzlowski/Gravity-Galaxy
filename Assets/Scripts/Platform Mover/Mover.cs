using UnityEngine;

public class Mover : MonoBehaviour
{
    [Header("Movement Settings")]
    public PointNetwork pointNetwork;
    public float speed = 5f;
    public float rotationSpeed = 3f;
    public bool resetToStart = false;

    [Header("State")]
    private int currentPointIndex = 0;
    private bool isMoving = false;
    private float waitTimeRemaining = 0f;

    [Header("Player Interaction")]
    private Transform player;
    private Rigidbody playerRb;
    private GroundDetection playerGroundDetection;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Rigidbody rb;
    private float maxPlayerDistance = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing from the platform.");
            return;
        }
        InitializePlatform();
    }

    private void FixedUpdate()
    {
        HandleMovementLogic();
        SyncPlayerMovement();
    }

    private void InitializePlatform()
    {
        if (pointNetwork != null && pointNetwork.points.Count > 0)
        {
            rb.position = pointNetwork.points[0].transform.position;
        }

        lastPosition = rb.position;
        lastRotation = rb.rotation;
    }

    private void HandleMovementLogic()
    {
        if (IsPointNetworkInvalid())
        {
            return;
        }

        UpdateWaitTime();

        if (waitTimeRemaining > 0)
        {
            return;
        }

        if (!isMoving)
        {
            HandleCurrentPointWaitTime();
        }

        MoveToNextPoint();
    }

    private bool IsPointNetworkInvalid()
    {
        return pointNetwork == null || pointNetwork.points.Count == 0;
    }

    private void UpdateWaitTime()
    {
        if (waitTimeRemaining > 0)
        {
            waitTimeRemaining -= Time.deltaTime;

            if (waitTimeRemaining <= 0)
            {
                isMoving = true;
            }
        }
    }

    private void HandleCurrentPointWaitTime()
    {
        Point currentPoint = pointNetwork.points[currentPointIndex];

        if (currentPoint.waitTime > 0)
        {
            waitTimeRemaining = currentPoint.waitTime;
        }
    }

    private void MoveToNextPoint()
    {
        Point targetPoint = pointNetwork.points[GetNextPointIndex()];
        MoveToPoint(targetPoint);
    }

    private void MoveToPoint(Point targetPoint)
    {
        Vector3 direction = (targetPoint.transform.position - rb.position).normalized;
        float distance = Vector3.Distance(rb.position, targetPoint.transform.position);

        float moveDistance = speed * Time.deltaTime;

        if (moveDistance >= distance)
        {
            rb.MovePosition(targetPoint.transform.position);
            isMoving = false;

            // Überprüfen, ob die Plattform am letzten Punkt ist
            if (!pointNetwork.loop && currentPointIndex == pointNetwork.points.Count - 1)
            {
                // Plattform bleibt am letzten Punkt stehen
                return;
            }

            // Nächsten Punkt ermitteln
            currentPointIndex = GetNextPointIndex();
        }
        else
        {
            rb.MovePosition(rb.position + direction * moveDistance);
        }

        RotateToTargetPoint(targetPoint);
    }

    private void RotateToTargetPoint(Point targetPoint)
    {
        Transform rotationTarget = targetPoint.targetObject != null ? targetPoint.targetObject : transform;
        Quaternion targetRotation = targetPoint.transform.rotation;

        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
    }

    private int GetNextPointIndex()
    {
        // Wenn Loop aktiviert ist, gehe zyklisch durch die Punkte
        if (pointNetwork.loop)
        {
            return (currentPointIndex + 1) % pointNetwork.points.Count;
        }

        // Wenn Loop deaktiviert ist, überprüfe, ob es der letzte Punkt ist
        if (currentPointIndex < pointNetwork.points.Count - 1)
        {
            return currentPointIndex + 1;
        }

        // Bleibe am letzten Punkt
        return currentPointIndex;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerRb = player.GetComponent<Rigidbody>();
            playerGroundDetection = player.GetComponent<GroundDetection>();
        }
    }

    private void SyncPlayerMovement()
    {
        if (player == null || player.GetComponent<Rigidbody>() == null)
        {
            return;
        }

        Vector3 platformMovement = transform.position - lastPosition;
        Quaternion platformRotationDelta = transform.rotation * Quaternion.Inverse(lastRotation);

        bool isPlayerOnPlatform = playerGroundDetection != null && playerGroundDetection.IsGrounded;

        float distanceToPlatform = Vector3.Distance(player.position, transform.position);

        if (isPlayerOnPlatform)
        {
            Vector3 playerOffset = playerRb.transform.position - transform.position;
            Vector3 rotatedOffset = platformRotationDelta * playerOffset;
            Vector3 movementDueToRotation = rotatedOffset - playerOffset;

            playerRb.MovePosition(playerRb.position + platformMovement + movementDueToRotation);

            playerRb.MoveRotation(Quaternion.Slerp(playerRb.rotation, transform.rotation, Time.fixedDeltaTime * rotationSpeed));
        }

        if (distanceToPlatform > maxPlayerDistance)
        {
            player = null;
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
}

