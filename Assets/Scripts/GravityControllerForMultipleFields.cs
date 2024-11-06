using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        // Finde die höchste Priorität unter den aktiven Gravitationsfeldern
        int highestPriority = activeGravityFields.Max(field => field.GetPriority());

        // Filtere nur die Gravitationsfelder mit der höchsten Priorität
        var highestPriorityFields = activeGravityFields
            .Where(field => field.GetPriority() == highestPriority)
            .ToList();

        if (highestPriorityFields.Count == 1)
        {
            // Nur ein Gravitationsfeld mit der höchsten Priorität
            GravityField gravityField = highestPriorityFields[0];
            Vector3 fieldGravityDirection = gravityField.CalculateGravityDirection(rb.position);
            float distance = Vector3.Distance(rb.position, gravityField.transform.position);
            float colliderRadius = gravityField.GetComponent<Collider>().bounds.extents.magnitude;
            float fieldGravityStrength;

            if (gravityField.GetGravityType() == GravityFieldType.Down)
            {
                // Verwende nur die gravityStrength
                fieldGravityStrength = gravityField.GetGravityStrength();
            }
            else
            {
                // Verwende die CalculateGravityForce Funktion oder die gravityStrength
                fieldGravityStrength = useGravityLaw
                    ? CalculateGravityForce(gravityField, rb, distance)
                    : gravityField.GetGravityStrength();

                if (distance > colliderRadius + 5)
                {
                    fieldGravityStrength = gravityField.GetGravityStrength();
                }
            }

            totalGravity = fieldGravityDirection * fieldGravityStrength;
            gravityDirection = fieldGravityDirection;
        }
        else
        {
            // Resultierende Gravitationskraft von mehreren Gravitationsfeldern mit der höchsten Priorität
            foreach (var gravityField in highestPriorityFields)
            {
                Vector3 fieldGravityDirection = gravityField.CalculateGravityDirection(rb.position);
                float distance = Vector3.Distance(rb.position, gravityField.transform.position);
                float fieldGravityStrength;

                if (gravityField.GetGravityType() == GravityFieldType.Down)
                {
                    fieldGravityStrength = gravityField.GetGravityStrength();
                }
                else
                {
                    fieldGravityStrength = useGravityLaw
                        ? CalculateGravityForce(gravityField, rb, distance)
                        : gravityField.GetGravityStrength();
                }

                totalGravity += fieldGravityDirection * fieldGravityStrength;
            }
            gravityDirection = totalGravity.normalized;
        }

        rb.AddForce(totalGravity, ForceMode.Acceleration);
    }

    private float CalculateGravityForce(GravityField gravityField, Rigidbody rb, float distance)
    {
        // Das Gravitationsgesetz:
        // F = G * (m1 * m2) / r^2
        // F: Gravitationskraft
        // G: Gravitationskonstante (hier als gravityField.GetGravityStregth() verwendet)
        // m1: Masse des ersten Objekts (hier rb.mass)
        // m2: Masse des zweiten Objekts (hier gravityField.GetGravityFieldMass())
        // r: Abstand zwischen den Mittelpunkten der beiden Objekte (hier distance)
        // Die Gravitationskraft nimmt mit dem Quadrat der Entfernung ab (r^2).

        return gravityField.GetGravityStrength()
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

    private void CheckGravityFieldDistances()
    {
        for (int i = activeGravityFields.Count - 1; i >= 0; i--)
        {
            GravityField gravityField = activeGravityFields[i];
            float distance = Vector3.Distance(transform.position, gravityField.transform.position);
            float radius = gravityField.GetGravityFieldRadius();
            if (distance > radius)
            {
                activeGravityFields.RemoveAt(i); // Feld entfernen, wenn außerhalb des Radius
            }
        }
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
            CheckGravityFieldDistances();
        }
    }

    void OnTriggerStay(Collider other)
    {
        CheckGravityFieldDistances();
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
