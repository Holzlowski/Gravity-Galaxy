using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityControllerForMultipleFields : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private List<GravityField> activeGravityFields = new List<GravityField>(); // Liste der aktiven Gravitationsfelder
    private Vector3 gravityDirection;

    [SerializeField]
    private float rotationToPlanetSpeed = 10f;
    private int lastAppliedPriority = -1;

    [SerializeField]
    private bool useGravityLaw = true;
    private GroundDetection groundDetection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundDetection = GetComponent<GroundDetection>();
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

        // Aktualisiere die Verzögerungstimer aller GravityFields
        foreach (var gravityField in activeGravityFields)
        {
            gravityField.UpdateDelayTimer(Time.deltaTime);
        }

        // Finde die höchste Priorität
        int currentHighestPriority = activeGravityFields.Max(field => field.Priority);

        // Überprüfe, ob sich die Priorität geändert hat
        if (currentHighestPriority != lastAppliedPriority)
        {
            lastAppliedPriority = currentHighestPriority;

            // Setze den Delay für alle Felder mit der neuen höchsten Priorität zurück
            foreach (var field in activeGravityFields)
            {
                if (field.Priority == currentHighestPriority)
                {
                    field.ResetDelay();
                }
            }
        }

        // Finde die höchste Priorität und filtere die relevanten Felder
        var highestPriorityFields = GetHighestPriorityFields();

        foreach (var gravityField in highestPriorityFields)
        {
            // Ignoriere Felder mit aktiver Verzögerung
            if (gravityField.IsDelayActive)
            {
                continue;
            }

            totalGravity += CalculateFieldGravity(gravityField);
        }
        gravityDirection = totalGravity.normalized;

        rb.AddForce(totalGravity, ForceMode.Acceleration);
    }

    private Vector3 CalculateFieldGravity(GravityField gravityField)
    {
        Vector3 fieldGravityDirection = gravityField.CalculateGravityDirection(rb.position);
        float distance = Vector3.Distance(rb.position, gravityField.transform.position);
        float colliderRadius = gravityField.GravityFieldRadius;
        float fieldGravityStrength;

        if (gravityField.GravityFieldType == GravityFieldType.TransformOneDirection)
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
        if (!groundDetection.IsGrounded && other.CompareTag("GravityField"))
        {
            GravityField gravityField = other.GetComponent<GravityField>();
            AddGravityField(gravityField);
            CheckGravityFieldDistances();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GravityField"))
        {
            RemoveGravityField(other.GetComponent<GravityField>());
            CheckGravityFieldDistances();
        }
    }

    private void AddGravityField(GravityField newGravityField)
    {
        if (!activeGravityFields.Contains(newGravityField))
        {
            newGravityField.ResetDelay(); // Verzögerung starten
            activeGravityFields.Add(newGravityField);
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

    public void GetClosestGravityField()
    {
        float minDistance = float.MaxValue;
        GravityField closestField = null;

        foreach (var gravityField in GetHighestPriorityFields())
        {
            float distance = Vector3.Distance(transform.position, gravityField.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestField = gravityField;
            }
        }

        if (closestField != null)
        {
            activeGravityFields.Clear();
            closestField.ResetDelay(); // Verzögerung aktivieren
            activeGravityFields.Add(closestField);
        }
    }
}

//  private void RotateAroundTheSun()
//     {
//         transform.RotateAround(
//             GameManager.Instance.Sun.transform.position,
//             Vector3.right,
//             rotationAroundSun * Time.deltaTime
//         );
//     }
