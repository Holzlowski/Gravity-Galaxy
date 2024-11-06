using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [SerializeField]
    private GravityFieldType gravityFieldType;
    private Collider gravityFieldCollider;

    [SerializeField]
    private float gravityStrength = 9.81f;

    [SerializeField]
    private float gravityFieldMass = 1000f;

    [SerializeField]
    private int priority = 0;

    void Awake()
    {
        gravityFieldCollider = GetComponent<Collider>();
    }

    public Vector3 CalculateGravityDirection(Vector3 playerPosition)
    {
        switch (gravityFieldType)
        {
            case GravityFieldType.Centerpoint:
                return (transform.position - playerPosition).normalized * gravityStrength;
            case GravityFieldType.Down:
                return transform.up * -1;
            case GravityFieldType.CenterpointInverse:
                return (playerPosition - transform.position).normalized * gravityStrength;
            default:
                return (transform.position - playerPosition).normalized;
        }
    }

    public float GetGravityStrength()
    {
        return gravityStrength;
    }

    public float GetGravityFieldMass()
    {
        return gravityFieldMass;
    }

    public float GetGravityFieldRadius()
    {
        return gravityFieldCollider.bounds.extents.magnitude;
    }

    public GravityFieldType GetGravityType()
    {
        return gravityFieldType;
    }

    public int GetPriority()
    {
        return priority;
    }
}

public enum GravityFieldType
{
    Centerpoint,
    Down,
    CenterpointInverse,
}
