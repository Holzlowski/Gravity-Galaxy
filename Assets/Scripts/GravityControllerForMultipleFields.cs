using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityControllerForMultipleFields : MonoBehaviour
{
    public List<GravityField> activeGravityFields = new List<GravityField>(); // Liste der aktiven Gravitationsfelder
    private Vector3 gravityDirection;
    public float rotationToPlanetSpeed = 10f;
    public bool useGravityLaw = true;

    public Vector3 GetGravityDirection()
    {
        return gravityDirection;
    }

    public void ApplyGravitation(Rigidbody rb)
    {
        Vector3 totalGravity = Vector3.zero;

        if (activeGravityFields.Count == 1)
        {
            // Richtung und Stärke des einzigen Gravitationsfeldes
            GravityField gravityField = activeGravityFields[0];
            Vector3 fieldGravityDirection = gravityField.CalculateGravityDirection(rb.position);
            float distance = Vector3.Distance(rb.position, gravityField.transform.position);
            float colliderRadius = gravityField.GetComponent<Collider>().bounds.extents.magnitude;
            float fieldGravityStrength = useGravityLaw
                ? CalculateGravityForce(gravityField, rb, distance)
                : gravityField.GetGravityStregth();

            // Erhöhe die Gravitationskraft, wenn der Spieler ein bisschen weiter entfernt ist als der Collider-Radius
            if (distance > colliderRadius + 5)
            {
                fieldGravityStrength = gravityField.GetGravityStregth(); // Erhöhe die Gravitationskraft um 50%
            }

            totalGravity = fieldGravityDirection * fieldGravityStrength;
            gravityDirection = fieldGravityDirection;
        }
        else
        {
            // Resultierende Gravitationskraft von mehreren Gravitationsfeldern
            foreach (var gravityField in activeGravityFields)
            {
                Vector3 fieldGravityDirection = gravityField.CalculateGravityDirection(rb.position);
                float distance = Vector3.Distance(rb.position, gravityField.transform.position);
                float fieldGravityStrength = useGravityLaw
                    ? CalculateGravityForce(gravityField, rb, distance)
                    : gravityField.GetGravityStregth();

                totalGravity += fieldGravityDirection * fieldGravityStrength; // Summe aller Gravitationskräfte
            }
            gravityDirection = totalGravity.normalized;
        }

        rb.AddForce(totalGravity, ForceMode.Acceleration);
    }

    private float CalculateGravityForce(GravityField gravityField, Rigidbody rb, float distance)
    {
        return gravityField.GetGravityStregth()
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

    private void OnTriggerEnter(Collider other)
    {
        if (
            GameManager.Instance.player.GetComponent<ThirdPersonController>().IsGrounded == false
            && other.CompareTag("GravityField")
        )
        {
            GravityField newGravityField = other.GetComponent<GravityField>();
            if (!activeGravityFields.Contains(newGravityField))
            {
                activeGravityFields.Add(newGravityField); // Neues Feld zur Liste hinzufügen
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GravityField"))
        {
            GravityField exitingGravityField = other.GetComponent<GravityField>();
            if (GameManager.Instance.allowSpaceFlight == false && activeGravityFields.Count == 1)
            {
                return;
            }
            else
            {
                activeGravityFields.Remove(exitingGravityField); // Feld beim Verlassen entfernen
            }
        }
    }
}
