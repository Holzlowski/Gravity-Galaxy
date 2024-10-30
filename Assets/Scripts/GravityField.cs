using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [SerializeField]
    private float gravityStrength = 9.81f;

    [SerializeField]
    private float gravityFieldMass = 1000f;

    public Vector3 CalculateGravityDirection(Vector3 playerPosition)
    {
        return (transform.position - playerPosition).normalized * gravityStrength;
    }

    public float GetGravityStregth()
    {
        return gravityStrength;
    }

    public float GetGravityFieldMass()
    {
        return gravityFieldMass;
    }
}
