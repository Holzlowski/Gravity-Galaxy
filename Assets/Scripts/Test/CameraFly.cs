using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFly : MonoBehaviour
{
    public float movementSpeed = 10f; // Geschwindigkeit der Bewegung
    public float lookSpeed = 2f; // Geschwindigkeit der Mausbewegung

    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        // Mausbewegung erfassen
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");

        // Kamera drehen
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // Bewegung erfassen
        float moveForward = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        float moveRight = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;

        // Kamera bewegen
        transform.Translate(moveRight, 0f, moveForward);
    }
}
