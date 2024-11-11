using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GravityControllerForMultipleFields : MonoBehaviour
{
    Rigidbody rb;
    public List<GravityField> activeGravityFields = new List<GravityField>(); // Liste der aktiven Gravitationsfelder
    private Vector3 gravityDirection;
    public float rotationToPlanetSpeed = 10f;
    public bool useGravityLaw = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public Vector3 GetGravityDirection()
    {
        return gravityDirection;
    }

    public void ApplyGravitation()
    {
        Vector3 totalGravity = Vector3.zero;

        if (activeGravityFields.Count == 0)
        {
            gravityDirection = Vector3.zero;
            rb.AddForce(totalGravity, ForceMode.Acceleration);
            return;
        }

        // Finde die höchste Priorität und filtere die relevanten Felder
        var highestPriorityFields = GetHighestPriorityFields();

        if (highestPriorityFields.Count == 1)
        {
            totalGravity = CalculateFieldGravity(highestPriorityFields[0]);
            gravityDirection = totalGravity.normalized;
        }
        else
        {
            foreach (var gravityField in highestPriorityFields)
            {
                totalGravity += CalculateFieldGravity(gravityField);
            }
            gravityDirection = totalGravity.normalized;
        }

        rb.AddForce(totalGravity, ForceMode.Acceleration);
    }

    private Vector3 CalculateFieldGravity(GravityField gravityField)
    {
        Vector3 fieldGravityDirection = gravityField.CalculateGravityDirection(rb.position);
        float distance = Vector3.Distance(rb.position, gravityField.transform.position);
        //float colliderRadius = gravityField.GetComponent<Collider>().bounds.extents.magnitude;
        float colliderRadius = gravityField.GravityFieldRadius;
        float fieldGravityStrength;

        if (gravityField.GravityType == GravityFieldType.Down)
        {
            fieldGravityStrength = gravityField.GravityStrength;
        }
        else
        {
            fieldGravityStrength = useGravityLaw
                ? CalculateGravityForce(gravityField, distance)
                : gravityField.GravityStrength;

            if (distance > colliderRadius + 5)
            {
                fieldGravityStrength = gravityField.GravityStrength;
            }
        }
        return fieldGravityDirection * fieldGravityStrength;
    }

    private float CalculateGravityForce(GravityField gravityField, float distance)
    {
        // Das Gravitationsgesetz:
        // F = G * (m1 * m2) / r^2
        // F: Gravitationskraft
        // G: Gravitationskonstante (hier als gravityField.GetGravityStregth() verwendet)
        // m1: Masse des ersten Objekts (hier rb.mass)
        // m2: Masse des zweiten Objekts (hier gravityField.GetGravityFieldMass())
        // r: Abstand zwischen den Mittelpunkten der beiden Objekte (hier distance)
        // Die Gravitationskraft nimmt mit dem Quadrat der Entfernung ab (r^2).

        return gravityField.GravityStrength
            * (rb.mass * gravityField.GravityFieldMass)
            / (distance * distance);
    }

    public void RotateToPlanet()
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
            float radius = gravityField.GravityFieldRadius;
            if (distance > radius)
            {
                activeGravityFields.RemoveAt(i); // Feld entfernen, wenn außerhalb des Radius
            }
        }
    }

    private List<GravityField> GetHighestPriorityFields()
    {
        int highestPriority = activeGravityFields.Max(field => field.Priority);
        return activeGravityFields.Where(field => field.Priority == highestPriority).ToList();
    }

    private void OnTriggerStay(Collider other)
    {
        CheckGravityFieldDistances();
        if (
            GameManager.Instance.player.GetComponent<ThirdPersonController>().IsGrounded == false
            && other.CompareTag("GravityField")
        )
        {
            AddGravityField(other.GetComponent<GravityField>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GravityField"))
        {
            RemoveGravityField(other.GetComponent<GravityField>());
        }
    }

    private void AddGravityField(GravityField newGravityField)
    {
        if (!activeGravityFields.Contains(newGravityField))
        {
            activeGravityFields.Add(newGravityField);
            CheckGravityFieldDistances();
        }
    }

    private void RemoveGravityField(GravityField gravityField)
    {
        if (GameManager.Instance.allowSpaceFlight == false && activeGravityFields.Count == 1)
        {
            return;
        }
        activeGravityFields.Remove(gravityField);
    }
}
