using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;
    public Transform cameraTransform;
    public float rotationSpeed = 1.0f;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void Start() { }

    void Update()
    {
        // Kamera um den Spieler rotieren lassen
        if (Input.GetKey(KeyCode.Q))
        {
            offset = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up) * offset;
        }
        if (Input.GetKey(KeyCode.E))
        {
            offset = Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.up) * offset;
        }

        Vector3 desiredPosition = target.position + target.rotation * offset;
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
        transform.position = smoothedPosition;


        // Blickrichtung der Kamera anpassen
        Quaternion targetRotation = Quaternion.LookRotation(
            target.position - transform.position,
            target.up
        );
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            smoothSpeed * Time.deltaTime
        );
        Debug.DrawLine(cameraTransform.position, cameraTransform.forward, Color.red);
    }
}

// interessante Kameraführung xD
// else
// {
//     desiredPosition = Vector3.Scale(target.position, offset);
// }
