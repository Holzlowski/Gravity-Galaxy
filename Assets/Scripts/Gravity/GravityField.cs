using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [SerializeField]
    private GravityFieldType gravityFieldType;
    private Collider gravityFieldCollider;

    [SerializeField]
    private Collider simpleCollider;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private float gravityStrength = 9.81f;

    [SerializeField]
    private float gravityFieldMass = 10f;

    private float gravityFieldRadius;

    [SerializeField]
    private int priority = 0;

    private KDTreeTriangle kdTree;

    private Vector3[] vertices;
    private Vector3[] normals;
    private List<Vector3[]> triangleList;

    void Awake()
    {
        gravityFieldCollider = GetComponent<Collider>();
        gravityFieldRadius = gravityFieldCollider.bounds.extents.magnitude;

        if (meshCollider != null)
        {
            Mesh mesh = meshCollider.sharedMesh;
            vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            triangleList = new List<Vector3[]>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3[] triangle = new Vector3[3];
                triangle[0] = meshCollider.transform.TransformPoint(vertices[triangles[i]]); // In Weltkoordinaten transformieren
                triangle[1] = meshCollider.transform.TransformPoint(vertices[triangles[i + 1]]);
                triangle[2] = meshCollider.transform.TransformPoint(vertices[triangles[i + 2]]);
                //Normalenvektor vom Mesh speichern

                triangleList.Add(triangle);
            }

            kdTree = new KDTreeTriangle(triangleList.ToArray());

            // // Zeichne Normalenvektoren von jedem Dreieck
            // foreach (var triangle in triangleList)
            // {
            //     // Berechne den Mittelpunkt des Dreiecks
            //     Vector3 center = (triangle[0] + triangle[1] + triangle[2]) / 3f;

            //     // Berechne die Normalen des Dreiecks
            //     Vector3 normal = CalculateTriangleNormal(triangle[0], triangle[1], triangle[2]);

            //     // Zeichne eine Linie vom Mittelpunkt des Dreiecks in Richtung des Normalenvektors
            //     Debug.DrawLine(center, center + normal * 0.1f, Color.green, 1000f);
            // }
        }

        //     if (meshCollider != null)
        //     {
        //         vertices = meshCollider.sharedMesh.vertices;
        //         normals = meshCollider.sharedMesh.normals;
        //         for (int i = 0; i < vertices.Length; i++)
        //         {
        //             vertices[i] = meshCollider.transform.TransformPoint(vertices[i]);
        //             Debug.DrawLine(vertices[i], vertices[i] + normals[i], Color.green, 1000f);
        //         }
        //         kdTree = new KDTree(vertices);
        //     }
    }

    // Berechnung des Normalenvektors eines Dreiecks
    Vector3 CalculateTriangleNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // Berechne die Kanten des Dreiecks
        Vector3 edge1 = v2 - v1;
        Vector3 edge2 = v3 - v1;

        // Berechne das Kreuzprodukt der Kanten, um den Normalenvektor zu bekommen
        Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

        return normal;
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
            case GravityFieldType.MeshBasedSimple:
                return CalculateMeshBasedGravity(playerPosition);
            case GravityFieldType.MeshBasedKDTree:
                return CalculateMeshBasedGravity(playerPosition);
            default:
                return (transform.position - playerPosition).normalized;
        }
    }

    private Vector3 CalculateMeshBasedGravity(Vector3 playerPosition)
    {
        if (simpleCollider != null)
        {
            Vector3 closestPoint = simpleCollider.ClosestPoint(playerPosition);
            //Debug.Log("ClosestPoint ist: " + closestPoint);
            return (closestPoint - playerPosition).normalized * gravityStrength;
        }
        else if (meshCollider != null)
        {
            // Finde das nächste Dreieck 
            Vector3[] closestTriangle = kdTree.FindNearestTriangle(playerPosition);

            // // Berechne die Mitte des Dreiecks 
            // Vector3 triangleCenter = (closestTriangle[0] + closestTriangle[1] + closestTriangle[2]) / 3.0f;

            // Zeichne eine Linie vom Spieler zur Mitte des Dreiecks
            //Debug.DrawLine(playerPosition, triangleCenter, Color.red, 1f);

            // Berechne die Normale des nächstgelegenen Dreiecks
            Vector3 normal = Vector3.Cross(closestTriangle[1] - closestTriangle[0], closestTriangle[2] - closestTriangle[0]).normalized;
            //Interpoliere die Normalen
            return -normal * gravityStrength;

            // Vector3 closestPoint = kdTree.FindNearest(playerPosition);
            // Debug.Log("Vertices: " + vertices.Length);
            // Debug.Log("Normals: " + normals.Length);
            // Debug.DrawLine(playerPosition, closestPoint, Color.red, 1f);

            // int closestIndex = System.Array.IndexOf(vertices, closestPoint);
            // Debug.Log("ClosestIndex ist: " + closestIndex);
            // Debug.Log("ClosestNormal ist: " + normals[closestIndex]);
            // Vector3 normal = normals[closestIndex];
            // Vector3 gravityDirection = vertices[closestIndex] + normal;
            // return -gravityDirection.normalized * gravityStrength;

            //return Vector3.zero;
        }
        else
        {
            Debug.LogError("MeshCollider is not set for MeshBased gravity field");
            return Vector3.zero;
        }
    }

    private Vector3 GetNormalFromMesh(Vector3 closestPoint)
    {
        // Finde den nächsten Punkt im Mesh und gib die Normalenrichtung zurück
        int closestIndex = System.Array.IndexOf(vertices, closestPoint);

        if (closestIndex >= 0 && closestIndex < normals.Length)
        {
            return normals[closestIndex];
        }
        else
        {
            Debug.LogError("Closest vertex index out of range.");
            return Vector3.zero; // Fallback-Normale
        }
    }

    // Vector3 CalculateNormalFromMesh(Vector3 closestPoint, Mesh mesh)
    // {
    //     // Hole die Mesh-Daten
    //     Vector3[] vertices = mesh.vertices;
    //     int[] triangles = mesh.triangles;


    //     // Suche das nächste Dreieck
    //     int nearestTriangle = -1;
    //     float minDistance = Mathf.Infinity;

    //     for (int i = 0; i < triangles.Length; i += 3)
    //     {
    //         Vector3 p1 = vertices[triangles[i]];
    //         Vector3 p2 = vertices[triangles[i + 1]];
    //         Vector3 p3 = vertices[triangles[i + 2]];

    //         Vector3 center = (p1 + p2 + p3) / 3.0f;
    //         float distance = Vector3.Distance(center, closestPoint);

    //         if (distance < minDistance)
    //         {
    //             minDistance = distance;
    //             nearestTriangle = i;
    //         }
    //     }

    //     // Berechne die Normale des Dreiecks
    //     if (nearestTriangle != -1)
    //     {
    //         Vector3 p1 = vertices[triangles[nearestTriangle]];
    //         Vector3 p2 = vertices[triangles[nearestTriangle + 1]];
    //         Vector3 p3 = vertices[triangles[nearestTriangle + 2]];

    //         Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
    //         Debug.Log("Normal: " + normal);
    //         return normal;
    //     }

    //     return Vector3.up; // Fallback-Normale
    // }

    // Vector3 CalculateNormalFromNeighbors(Vector3 closestPoint, KDTree kdTree)
    // {
    //     // Suche die nächsten Punkte im KD-Tree
    //     Vector3[] neighbors = kdTree.FindKNearest(closestPoint, 5).ToArray();

    //     if (neighbors.Length < 3)
    //         return Vector3.up; // Fallback-Normale, wenn es zu wenige Punkte gibt

    //     // Berechne die Normalen durch Kreuzprodukt
    //     Vector3 normal = Vector3.zero;
    //     for (int i = 0; i < neighbors.Length - 1; i++)
    //     {
    //         Vector3 p1 = neighbors[i];
    //         Vector3 p2 = neighbors[i + 1];
    //         normal += Vector3.Cross(p1 - closestPoint, p2 - closestPoint);
    //     }

    //     return -normal.normalized;
    // }

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
    MeshBasedSimple,
    MeshBasedKDTree,
}
