using UnityEngine;

public class Mover : MonoBehaviour
{
    public PointNetwork pointNetwork;
    public float speed = 5f; // Geschwindigkeit der Bewegung
    public float rotationSpeed = 3f; // Geschwindigkeit der Drehung
    public bool resetToStart = false; // Ob das Objekt nach Erreichen des letzten Punktes zum Anfang zurückkehren soll
    private int currentPointIndex = 0;
    private bool isMoving = false;
    private float waitTimeRemaining = 0f;

    private Vector3 lastPosition; // Letzte Position der Plattform
    private Quaternion lastRotation; // Letzte Rotation der Plattform
    private GameObject player; // Referenz zum Spieler

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        if (pointNetwork != null && pointNetwork.points.Count > 0)
        {
            transform.position = pointNetwork.points[0].transform.position;
        }
    }

    void Update()
    {
        if (pointNetwork == null || pointNetwork.points.Count == 0)
        {
            return; // Abbruch, wenn kein Punktnetz vorhanden ist
        }

        // Überprüfung der verbleibenden Wartezeit
        if (waitTimeRemaining > 0)
        {
            waitTimeRemaining -= Time.deltaTime;

            // Bewegung starten, wenn die Wartezeit abgelaufen ist
            if (waitTimeRemaining <= 0)
            {
                isMoving = true;
            }
            return; // Abbruch, wenn noch gewartet wird
        }

        if (!isMoving)
        {
            Point currentPoint = pointNetwork.points[currentPointIndex];

            // Überprüfung, ob der aktuelle Punkt eine Wartezeit hat
            if (currentPoint.waitTime > 0)
            {
                waitTimeRemaining = currentPoint.waitTime;
                return; // Abbruch, wenn gewartet werden muss
            }

            // Bewegung starten, falls keine Wartezeit vorhanden ist
            isMoving = true;
        }

        Point targetPoint = pointNetwork.points[(currentPointIndex + 1) % pointNetwork.points.Count];
        MoveToPoint(targetPoint);
    }

    private void FixedUpdate()
    {
        // Synchronisation des Spielers, wenn sich dieser auf der Plattform befindet
        if (player != null)
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();

            // Berechnung der Plattformbewegung
            Vector3 platformMovement = transform.position - lastPosition;

            // Anwendung der Plattformbewegung auf den Spieler
            playerRb.MovePosition(playerRb.position + platformMovement);

            // Optionale Anwendung der Plattformrotation auf den Spieler
            Quaternion platformRotationDelta = transform.rotation * Quaternion.Inverse(lastRotation);
            Vector3 playerOffset = playerRb.position - transform.position;
            Vector3 rotatedOffset = platformRotationDelta * playerOffset;
            Vector3 movementDueToRotation = rotatedOffset - playerOffset;
            playerRb.MovePosition(playerRb.position + movementDueToRotation);

            // Drehung des Spielers entsprechend der Plattform
            playerRb.MoveRotation(Quaternion.Slerp(playerRb.rotation, transform.rotation, Time.deltaTime * speed));
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void MoveToPoint(Point targetPoint)
    {
        Vector3 direction = (targetPoint.transform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPoint.transform.position);
        float moveDistance = speed * Time.deltaTime;

        // Bewegung der Plattform in Richtung des Zielpunkts
        if (moveDistance >= distance)
        {
            transform.position = targetPoint.transform.position;
            isMoving = false;
            currentPointIndex = (currentPointIndex + 1) % pointNetwork.points.Count;

            if (!pointNetwork.loop && currentPointIndex == 0)
            {
                if (resetToStart)
                {
                    currentPointIndex = 0; // Zurück zum Anfang
                }
                else
                {
                    enabled = false; // Bewegung beenden, wenn kein Loop vorhanden ist und das Ende erreicht ist
                }
            }
        }
        else
        {
            transform.position += direction * moveDistance;
        }

        // Drehung der Plattform zur Zielausrichtung
        RotateToTargetPoint(targetPoint);
    }

    private void RotateToTargetPoint(Point targetPoint)
    {
        // Berechnung der Zielrotation
        Quaternion targetRotation = targetPoint.transform.rotation;

        // Drehung der Plattform zur Zielrotation mit der angegebenen Drehgeschwindigkeit
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject; // Referenz zum Spieler speichern
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null; // Referenz zum Spieler zurücksetzen
        }
    }
}
