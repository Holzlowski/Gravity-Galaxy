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
    private GameObject player;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        InitializePlatform();
    }

    void Update()
    {
        HandleMovementLogic();
    }

    private void FixedUpdate()
    {
        SyncPlayerMovement();
    }

    private void InitializePlatform()
    {
        if (pointNetwork != null && pointNetwork.points.Count > 0)
        {
            transform.position = pointNetwork.points[0].transform.position;
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
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

    private void SyncPlayerMovement()
    {
        if (player == null)
        {
            return;
        }

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null)
        {
            return;
        }

        Vector3 platformMovement = transform.position - lastPosition;
        Quaternion platformRotationDelta = transform.rotation * Quaternion.Inverse(lastRotation);

        Vector3 playerOffset = playerRb.position - transform.position;
        Vector3 rotatedOffset = platformRotationDelta * playerOffset;
        Vector3 movementDueToRotation = rotatedOffset - playerOffset;

        playerRb.MovePosition(playerRb.position + platformMovement + movementDueToRotation);
        playerRb.MoveRotation(Quaternion.Slerp(playerRb.rotation, transform.rotation, Time.fixedDeltaTime * rotationSpeed));

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void MoveToPoint(Point targetPoint)
    {
        Vector3 direction = (targetPoint.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPoint.transform.position);
        float moveDistance = speed * Time.deltaTime;

        if (moveDistance >= distance)
        {
            transform.position = targetPoint.transform.position;
            isMoving = false;
            currentPointIndex = GetNextPointIndex();

            if (!pointNetwork.loop && currentPointIndex == 0)
            {
                HandleEndOfPath();
            }
        }
        else
        {
            transform.position += direction * moveDistance;
        }

        RotateToTargetPoint(targetPoint);
    }

    private void HandleEndOfPath()
    {
        if (resetToStart)
        {
            currentPointIndex = 0;
        }
        else
        {
            enabled = false;
        }
    }

    private void RotateToTargetPoint(Point targetPoint)
    {
        Transform rotationTarget = targetPoint.targetObject != null ? targetPoint.targetObject : transform;
        Quaternion targetRotation = targetPoint.transform.rotation;

        rotationTarget.rotation = Quaternion.Slerp(rotationTarget.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private int GetNextPointIndex()
    {
        return (currentPointIndex + 1) % pointNetwork.points.Count;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }
}
