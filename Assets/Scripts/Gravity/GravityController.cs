using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private List<GravityField> activeGravityFields = new List<GravityField>(); // Liste der aktiven Gravitationsfelder
    private Vector3 gravityDirection;
    private GravityState gravityState;

    [SerializeField]
    private float rotationToPlanetSpeed = 10f;
    [SerializeField]
    private float pushBackForce = 10f;
    private int lastAppliedPriority = -1;

    [SerializeField]
    private bool useGravityLaw = true;
    private GroundDetection groundDetection;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundDetection = GetComponent<GroundDetection>();
        gravityState = new GravityState();
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

        var highestPriorityFields = GetHighestPriorityFields();
        int currentHighestPriority = highestPriorityFields.First().Priority;

        if (currentHighestPriority != lastAppliedPriority)
        {
            lastAppliedPriority = currentHighestPriority;

            foreach (var field in highestPriorityFields)
            {
                field.ResetDelay();
            }
        }

        foreach (var gravityField in highestPriorityFields)
        {
            gravityField.UpdateDelayTimer(Time.deltaTime);

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
        Vector3 fieldGravityDirection = gravityField.CalculateGravityDirection(rb.position, gravityState);
        float distance = Vector3.Distance(rb.position, gravityField.transform.position);
        float colliderRadius = gravityField.GravityFieldRadius;
        float fieldGravityStrength;

        if (gravityField.GravityFieldType == GravityFieldType.TransformOneDirection
        || gravityField.GravityFieldType == GravityFieldType.OneDirection)
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
                fieldGravityStrength = pushBackForce;
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

    private List<GravityField> GetHighestPriorityFields()
    {
        int highestPriority = activeGravityFields.Max(field => field.Priority);
        return activeGravityFields.Where(field => field.Priority == highestPriority).ToList();
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

    private void OnTriggerStay(Collider other)
    {
        GravityField gravityField = other.GetComponent<GravityField>();
        if (gravityField == null) return;

        bool isGrounded = groundDetection.IsGrounded;
        bool isGravityField = other.CompareTag("GravityField");
        bool hasHigherPriority = activeGravityFields.Count == 0
        || gravityField.Priority > activeGravityFields.Max(field => field.Priority);

        if (!isGrounded && isGravityField || hasHigherPriority)
        {
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
}

//  private void RotateAroundTheSun()
//     {
//         transform.RotateAround(
//             GameManager.Instance.Sun.transform.position,
//             Vector3.right,
//             rotationAroundSun * Time.deltaTime
//         );
//     }
