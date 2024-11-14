using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [SerializeField]
    private GravityFieldType gravityFieldType;
    private Collider gravityFieldCollider;

    [SerializeField]
    private Collider meshCollider;

    [SerializeField]
    private float gravityStrength = 9.81f;

    [SerializeField]
    private float gravityFieldMass = 10f;

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
            case GravityFieldType.MeshBased:
                return CalculateMeshBasedGravity(playerPosition);
            default:
                return (transform.position - playerPosition).normalized;
        }
    }

    private Vector3 CalculateMeshBasedGravity(Vector3 playerPosition)
    {
        if (meshCollider != null)
        {
            Vector3 closestPoint = meshCollider.ClosestPoint(playerPosition);
            return (closestPoint - playerPosition).normalized * gravityStrength;
        }
        else
        {
            Debug.LogError("MeshCollider is not set for MeshBased gravity field");
            return Vector3.zero;
        }
    }

    public float GravityStrength => gravityStrength;
    public float GravityFieldMass => gravityFieldMass;
    public float GravityFieldRadius => gravityFieldRadius;
    public GravityFieldType GravityFieldType => gravityFieldType;
    public int Priority => priority;
}

public enum GravityFieldType
{
    Centerpoint,
    Down,
    CenterpointInverse,
    MeshBased,
}
