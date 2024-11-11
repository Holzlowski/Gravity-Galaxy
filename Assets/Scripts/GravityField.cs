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

    private float gravityFieldRadius;

    [SerializeField]
    private int priority = 0;

    void Awake()
    {
        gravityFieldCollider = GetComponent<Collider>();
        gravityFieldRadius = gravityFieldCollider.bounds.extents.magnitude;
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

    public float GravityStrength => gravityStrength;
    public float GravityFieldMass => gravityFieldMass;
    public float GravityFieldRadius => gravityFieldRadius;
    public GravityFieldType GravityType => gravityFieldType;
    public int Priority => priority;
}

public enum GravityFieldType
{
    Centerpoint,
    Down,
    CenterpointInverse,
}
