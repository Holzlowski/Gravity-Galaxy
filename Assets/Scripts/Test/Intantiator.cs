using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateAtColliderEdge : MonoBehaviour
{
    [Header("Object to Instantiate")]
    public GameObject objectToInstantiate;

    [Header("Collider Settings")]
    public Collider targetCollider;

    [Header("Instantiation Settings")]
    public int numberOfObjects = 10;
    public float heightOffset = 0.1f;

    private void Start()
    {
        if (targetCollider == null || objectToInstantiate == null)
        {
            Debug.LogError("Please assign a Collider and an Object to Instantiate.");
            return;
        }

        if (targetCollider is BoxCollider boxCollider)
        {
            InstantiateAroundBoxCollider(boxCollider);
        }
        else if (targetCollider is SphereCollider sphereCollider)
        {
            InstantiateAroundSphereCollider(sphereCollider);
        }
        else
        {
            Debug.LogError("Only BoxCollider and SphereCollider are supported.");
        }
    }

    private void InstantiateAroundBoxCollider(BoxCollider boxCollider)
    {
        Vector3 colliderCenter = boxCollider.center;
        Vector3 colliderSize = boxCollider.size;

        for (int i = 0; i < numberOfObjects; i++)
        {
            float t = (float)i / numberOfObjects;
            float angle = t * Mathf.PI * 2; // Full circle

            // Calculate position on the box perimeter (XY plane example)
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * colliderSize.x / 2,
                0,
                Mathf.Sin(angle) * colliderSize.z / 2
            );

            Vector3 worldPosition = boxCollider.transform.TransformPoint(colliderCenter + offset);
            worldPosition.y += heightOffset; // Apply height offset

            Instantiate(objectToInstantiate, worldPosition, Quaternion.identity);
        }
    }

    private void InstantiateAroundSphereCollider(SphereCollider sphereCollider)
    {
        Vector3 colliderCenter = sphereCollider.center;
        float radius = sphereCollider.radius;

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Fibonacci Sphere formula for evenly distributed points
            float phi = Mathf.Acos(1 - 2 * (i + 0.5f) / numberOfObjects); // Latitude
            float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * i;             // Longitude

            float x = Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = Mathf.Cos(phi);

            // Calculate position on the sphere
            Vector3 offset = new Vector3(x, y, z) * radius;

            // Convert local position to world position
            Vector3 worldPosition = sphereCollider.transform.TransformPoint(colliderCenter + offset);

            // Instantiate object
            Instantiate(objectToInstantiate, worldPosition, Quaternion.identity);
        }
    }
}
