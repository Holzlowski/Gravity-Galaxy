using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    private Vector3 gravityDirection;
    public float rotationToPlanetSpeed = 10f;
    private float gravityStrength;
    public Transform currentGravityField;

    GravityField gravityField;
    private Vector3 surfaceNormal;

    public Vector3 GetGravityDirection()
    {
        return gravityDirection;
    }

    private void Awake()
    {
        gravityField = currentGravityField.GetComponent<GravityField>();
        gravityStrength = gravityField.GetGravityStregth();
        gravityDirection = gravityField.CalculateGravityDirection(transform.position);
    }

    public void ApplyGravitation(Rigidbody rb)
    {
        if (GameManager.Instance.useNormals)
        {
            // Verwende die entgegengesetzte Richtung der Normalen
            gravityDirection = -surfaceNormal.normalized * gravityStrength;
            rb.AddForce(gravityDirection, ForceMode.Acceleration);
        }
        else if (currentGravityField != null)
        {
            // Verwende die Richtung zum Zentrum des Gravitationsfeldes
            gravityDirection =
                // (currentGravityField.position - rb.position).normalized * gravityStrength;
                gravityField.CalculateGravityDirection(rb.position);

            if (GameManager.Instance.useGravityLaw == true)
            {
                // Berechne die Entfernung zum Zentrum des Gravitationsfeldes
                float distance = Vector3.Distance(rb.position, currentGravityField.position);
                float gravityForce = CalculateGravityForce(rb, distance);

                // Wende die Gravitationskraft an
                rb.AddForce(gravityDirection * gravityForce, ForceMode.Acceleration);
            }
            else
            {
                // Konstante Gravitationskraft an
                rb.AddForce(gravityDirection, ForceMode.Acceleration);
            }
        }
    }

    private float CalculateGravityForce(Rigidbody rb, float distance)
    {
        // Berechne die Gravitationskraft basierend auf der Entfernung und den Massen
        // F = G * (m1 * m2) / r^2
        return gravityStrength
            * (rb.mass * gravityField.GetGravityFieldMass())
            / (distance * distance);
    }

    public void RotateToPlanet(Rigidbody rb)
    {
        Quaternion upRotation = Quaternion.FromToRotation(transform.up, -gravityDirection);
        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            upRotation * rb.rotation,
            Time.fixedDeltaTime * rotationToPlanetSpeed
        );
        rb.MoveRotation(newRotation);
    }

    public void PerformRaycastToPlanet()
    {
        RaycastHit hit;
        Vector3 directionToPlanet = (currentGravityField.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToPlanet, out hit))
        {
            surfaceNormal = hit.normal;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance.switchGravityFieldBasedOnDistance == true)
            return;

        if (other.transform == currentGravityField)
        {
            return;
        }

        if (other.CompareTag("GravityField"))
        {
            UpdateGravityField(other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance.switchGravityFieldBasedOnDistance == false)
            return;

        if (other.transform == currentGravityField)
        {
            return;
        }

        if (other.CompareTag("GravityField"))
        {
            HandleGravityField(other.transform);
        }
    }

    private void UpdateGravityField(Transform newGravityField)
    {
        gravityField = newGravityField.GetComponent<GravityField>();
        gravityStrength = gravityField.GetGravityStregth();
        gravityDirection = gravityField.CalculateGravityDirection(transform.position);
        currentGravityField = newGravityField;
    }

    private void HandleGravityField(Transform newGravityField)
    {
        if (currentGravityField != null)
        {
            float distanceToCurrentPlanet = Vector3.Distance(
                transform.position,
                currentGravityField.position
            );

            float distanceToNewPlanet = Vector3.Distance(
                transform.position,
                newGravityField.position
            );

            // Ändere den currentPlanet nur, wenn der Spieler näher am neuen Planeten ist
            if (distanceToNewPlanet < distanceToCurrentPlanet)
            {
                UpdateGravityField(newGravityField);
            }
        }
        else
        {
            UpdateGravityField(newGravityField);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameManager.Instance.allowSpaceFlight && other.transform == currentGravityField)
        {
            currentGravityField = null;
            gravityField = null;
            gravityDirection = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (GameManager.Instance.useNormals && collision.gameObject.CompareTag("Ground"))
        {
            // Erhalte den Normalvektor der Oberfläche
            surfaceNormal = collision.contacts[0].normal;
        }
    }
}
