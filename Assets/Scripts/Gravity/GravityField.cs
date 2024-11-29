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

    private Triangle currentClosestTriangle;
    private Vector3 currentTriangleCenter;

    [SerializeField]
    private float thresholdDistance = 0.1f;

    void Awake()
    {
        gravityFieldCollider = GetComponent<Collider>();
        gravityFieldRadius = gravityFieldCollider.bounds.extents.magnitude;

        if (meshCollider != null)
        {
            Mesh mesh = meshCollider.sharedMesh;
            vertices = mesh.vertices;
            normals = mesh.normals;

            // kdTree = new KDTree(vertices);
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

            // Zeichne Normalenvektoren von jedem Dreieck
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
            Debug.Log(Vector3.Distance(playerPosition, currentTriangleCenter));
            // Überprüfe, ob der Spieler eine bestimmte Distanz vom Mittelpunkt des aktuellen Dreiecks überschreitet
            if (currentClosestTriangle == null || Vector3.Distance(playerPosition, currentTriangleCenter) > thresholdDistance)
            {
                // Verwende den KDTree, um das nächstgelegene Dreieck zu finden
                currentClosestTriangle = kdTree.FindNearestTriangleNode(playerPosition);
                if (currentClosestTriangle != null)
                {
                    currentTriangleCenter = currentClosestTriangle.GetCenter();
                }
            }

            if (currentClosestTriangle != null)
            {
                // Berechne die Gravitationsrichtung basierend auf der Normalen des nächstgelegenen Dreiecks
                Vector3 interpolatedNormal = currentClosestTriangle.normal;
                float totalWeight = 1.0f;

                // Zeichne eine Linie vom Mittelpunkt des nächstgelegenen Dreiecks in Richtung der Normalen
                Debug.DrawLine(currentTriangleCenter, currentTriangleCenter + currentClosestTriangle.normal, Color.red);

                // Interpoliere die Normalen der Nachbarn basierend auf dem Abstand
                foreach (var neighbor in currentClosestTriangle.neighbors)
                {
                    // Zeichne eine Linie vom Mittelpunkt des Nachbar-Dreiecks in Richtung der Normalen
                    Vector3 neighborCenter = neighbor.GetCenter();
                    Debug.DrawLine(neighborCenter, neighborCenter + neighbor.normal, Color.blue);

                    float distance = Vector3.Distance(playerPosition, neighborCenter);
                    float weight = 1f / (distance + 0.001f); // Vermeide Division durch Null
                    interpolatedNormal = Vector3.Lerp(interpolatedNormal, neighbor.normal, weight);
                    totalWeight += weight;
                }

                interpolatedNormal /= totalWeight;

                // Berechne die entgegengesetzte Richtung der interpolierten Normale als Gravitation
                Vector3 gravityDirection = -interpolatedNormal.normalized * gravityStrength;

                return gravityDirection;
            }
            else
            {
                Debug.LogError("No closest triangle found in KDTree");
                return Vector3.zero;
            }
        }
        else
        {
            Debug.LogError("MeshCollider is not set for MeshBased gravity field");
            return Vector3.zero;
        }
    }

    // Hilfsfunktion, um den nächstgelegenen Punkt auf einer Linie zu finden
    private Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 line = lineEnd - lineStart;
        float lineLengthSquared = line.sqrMagnitude;
        if (lineLengthSquared == 0.0f)
        {
            return lineStart;
        }

        float t = Vector3.Dot(point - lineStart, line) / lineLengthSquared;
        t = Mathf.Clamp01(t);
        return lineStart + t * line;
    }

    private float DistanceToTriangle(Vector3 point, Vector3[] triangle)
    {
        Vector3 triangleCenter = (triangle[0] + triangle[1] + triangle[2]) / 3.0f;
        return Vector3.Distance(point, triangleCenter);
    }

    public float DistanceToEdge(Vector3 point, Vector3 edgeStart, Vector3 edgeEnd)
    {
        Vector3 edge = edgeEnd - edgeStart;
        Vector3 pointToStart = point - edgeStart;

        float edgeLengthSquared = edge.sqrMagnitude;
        if (edgeLengthSquared == 0.0f)
        {
            return pointToStart.magnitude;
        }

        float t = Vector3.Dot(pointToStart, edge) / edgeLengthSquared;
        t = Mathf.Clamp01(t);

        Vector3 projection = edgeStart + t * edge;
        return (point - projection).magnitude;
    }

    private Vector3 InterpolateNormalsAtEdge(Vector3 playerPosition, Triangle triangleA, Triangle triangleB, Vector3 edgeStart, Vector3 edgeEnd, float gravityStrength)
    {
        // Berechne die Position des Spielers relativ zur Kante
        float t = Vector3.Dot(playerPosition - edgeStart, edgeEnd - edgeStart) / (edgeEnd - edgeStart).sqrMagnitude;
        t = Mathf.Clamp01(t); // Stelle sicher, dass t im Bereich [0, 1] liegt

        // Interpoliere die Normalen der beiden Dreiecke
        Vector3 interpolatedNormal = Vector3.Lerp(triangleA.normal, triangleB.normal, t).normalized;

        return -interpolatedNormal * gravityStrength;
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


//DER CODE HAT FUNKTIONIERT, ABER ES GAB PROBLEME MIT DER INTERPOLATION
//  // // Finde das nächste Dreieck
//             // Triangle closestNode = kdTree.FindNearestTriangleNode(playerPosition);
//             // Vector3[] closestTriangle = closestNode.vertices;

//             // // Berechne die baryzentrischen Koordinaten des Spielerpunktes
//             // Vector3 baryCoords = kdTree.CalculateBarycentricCoordinates(playerPosition, closestTriangle[0], closestTriangle[1], closestTriangle[2]);

//             // // Interpoliere die Normalen der Dreiecke
//             // Vector3 interpolatedNormal = closestNode.normal * baryCoords.x;

//             // // Berücksichtige die benachbarten Dreiecke
//             // foreach (Triangle neighbor in closestNode.neighbors)
//             // {
//             //     Vector3 neighborBaryCoords = kdTree.CalculateBarycentricCoordinates(playerPosition, neighbor.vertices[0], neighbor.vertices[1], neighbor.vertices[2]);
//             //     interpolatedNormal = Vector3.Lerp(interpolatedNormal, neighbor.normal, neighborBaryCoords.x);
//             // }

//             // interpolatedNormal = interpolatedNormal.normalized;

//             // return -interpolatedNormal * gravityStrength;

//             // // Finde das nächste Dreieck 
//             // Triangle closestTriangle = kdTree.FindNearestTriangleNode(playerPosition);
//             // //Vector3[] closestTriangleVertices = closestTriangle.vertices;

//             // // // Berechne die Mitte des Dreiecks 
//             // // Vector3 triangleCenter = (closestTriangle[0] + closestTriangle[1] + closestTriangle[2]) / 3.0f;

//             // // Zeichne eine Linie vom Spieler zur Mitte des Dreiecks
//             // //Debug.DrawLine(playerPosition, triangleCenter, Color.red, 1f);

//             // // Berechne die Normale des nächstgelegenen Dreiecks
//             // //Vector3 normal = Vector3.Cross(closestTriangleVertices[1] - closestTriangleVertices[0], closestTriangleVertices[2] - closestTriangleVertices[0]).normalized;
//             // //Interpoliere die Normalen
//             // return -closestTriangle.normal * gravityStrength;

//             // Definiere eine Schwellenentfernung für die Interpolation
//             //float thresholdDistance = 0.1f; // Passe diesen Wert nach Bedarf an

//             // Finde das nächste Dreieck
//             Triangle closestTriangle = kdTree.FindNearestTriangleNode(playerPosition);
//             List<Triangle> neighbors = closestTriangle.neighbors;

//             //Debug.DrawLine(closestTriangle.CalculateCenter(), closestTriangle.CalculateCenter() + closestTriangle.normal, Color.blue, 1f);

//             // Distanzen des Spielers zu den Nachbarn und deren Kanten
//             float minEdgeDistance = float.MaxValue;
//             Triangle closestNeighbor = null;
//             Vector3 closestEdgeStart = Vector3.zero;
//             Vector3 closestEdgeEnd = Vector3.zero;

//             foreach (Triangle neighbor in neighbors)
//             {
//                 Debug.DrawLine(neighbor.CalculateCenter(), neighbor.CalculateCenter() + neighbor.normal, Color.red, 1f);

//                 for (int i = 0; i < 3; i++)
//                 {
//                     Vector3 edgeStart = neighbor.vertices[i];
//                     Vector3 edgeEnd = neighbor.vertices[(i + 1) % 3];
//                     float edgeDistance = DistanceToEdge(playerPosition, edgeStart, edgeEnd);
//                     if (edgeDistance < minEdgeDistance)
//                     {
//                         minEdgeDistance = edgeDistance;
//                         closestNeighbor = neighbor;
//                         closestEdgeStart = edgeStart;
//                         closestEdgeEnd = edgeEnd;
//                     }
//                 }
//             }

//             // Zeichne eine Linie vom Spieler zur nächstgelegenen Kante
//             if (closestNeighbor != null)
//             {
//                 Vector3 closestPointOnEdge = ClosestPointOnLineSegment(playerPosition, closestEdgeStart, closestEdgeEnd);
//                 Debug.DrawLine(playerPosition, closestPointOnEdge, Color.green, 1f);
//             }

//             // Interpolation der Normalen der Dreiecke mit Slerp
//             Vector3 interpolatedNormal;
//             if (closestNeighbor != null && minEdgeDistance < thresholdDistance)
//             {
//                 Debug.DrawLine(closestNeighbor.CalculateCenter(), closestNeighbor.CalculateCenter() + closestNeighbor.normal, Color.yellow, 1f);
//                 interpolatedNormal = Vector3.Slerp(closestTriangle.normal, closestNeighbor.normal, minEdgeDistance / thresholdDistance);
//             }
//             else
//             {
//                 interpolatedNormal = closestTriangle.normal;
//             }

//             return -interpolatedNormal * gravityStrength;

//             // // Wenn der Spieler nicht nahe einer Kante ist, nutze die Normale des aktuellen Dreiecks
//             // return -closestTriangle.normal * gravityStrength;

//------------------------------------OLD CODE------------------------------------

//VERSUCH NUR AN DEN KANTEN ZU INTERPOLIEREN
//Vector3[] vertices = closestTriangle.vertices;

// // Berechne die Distanzen des Spielers zu den drei Kanten des aktuellen Dreiecks
// float distToEdge1 = DistanceToEdge(playerPosition, vertices[0], vertices[1]);
// float distToEdge2 = DistanceToEdge(playerPosition, vertices[1], vertices[2]);
// float distToEdge3 = DistanceToEdge(playerPosition, vertices[2], vertices[0]); 

// // Schwellenwert: Spieler ist "nahe genug" an einer Kante
// float edgeThreshold = 0.1f;

// // Bestimme die nächstgelegene Kante und ihren Nachbarn
// if (distToEdge1 < edgeThreshold && neighbors[0] != null)
// {
//     Debug.Log("Edge 1");
//     return InterpolateNormalsAtEdge(playerPosition, closestTriangle, neighbors[0], vertices[0], vertices[1], gravityStrength);
// }
// else if (distToEdge2 < edgeThreshold && neighbors[1] != null)
// {
//     Debug.Log("Edge 2");
//     return InterpolateNormalsAtEdge(playerPosition, closestTriangle, neighbors[1], vertices[1], vertices[2], gravityStrength);
// }
// else if (distToEdge3 < edgeThreshold && neighbors[2] != null)
// {
//     Debug.Log("Edge 3");
//     return InterpolateNormalsAtEdge(playerPosition, closestTriangle, neighbors[2], vertices[2], vertices[0], gravityStrength);
// }


// // Finde das nächste Dreieck
// Vector3[] closestTriangle = kdTree.FindNearestTriangle(playerPosition);

// // Berechne die Normale des nächstgelegenen Dreiecks
// Vector3 triangleNormal = Vector3.Cross(closestTriangle[1] - closestTriangle[0], closestTriangle[2] - closestTriangle[0]).normalized;

// // Berechne die baryzentrischen Koordinaten des Spielerpunktes
// Vector3 baryCoords = CalculateBarycentricCoordinates(playerPosition, closestTriangle[0], closestTriangle[1], closestTriangle[2]);

// int vertexIndex0 = System.Array.IndexOf(vertices, closestTriangle[0]);
// int vertexIndex1 = System.Array.IndexOf(vertices, closestTriangle[1]);
// int vertexIndex2 = System.Array.IndexOf(vertices, closestTriangle[2]);

// // Prüfe, ob die Indizes korrekt gefunden wurden
// Debug.Assert(vertexIndex0 != -1 && vertexIndex1 != -1 && vertexIndex2 != -1, "Vertex nicht gefunden!");

// // Hole die zugehörigen Normalen
// Vector3 vertexNormal0 = normals[vertexIndex0];
// Vector3 vertexNormal1 = normals[vertexIndex1];
// Vector3 vertexNormal2 = normals[vertexIndex2];

// // Interpoliere die Normalen
// Vector3 interpolatedNormal = (vertexNormal0 * baryCoords.x +
//                               vertexNormal1 * baryCoords.y +
//                               vertexNormal2 * baryCoords.z).normalized;

// // Kombiniere die Dreiecks-Normale und die interpolierte Vertex-Normale
// Vector3 gravityDirection = Vector3.Lerp(triangleNormal, interpolatedNormal, 0.5f).normalized;

// // Rückgabe der kombinierten Richtung als Gravitationsvektor
// return -gravityDirection * gravityStrength;


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