using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [Header("Gravity Field Settings")]
    [SerializeField] private GravityFieldType gravityFieldType;
    [SerializeField] private float gravityStrength = 9.81f;
    [SerializeField] private float gravityFieldMass = 10f;
    [SerializeField] private int priority = 0;
    private float gravityFieldRadius;

    [Header("Colliders")]
    [SerializeField] private Collider simpleCollider;
    [SerializeField] private MeshCollider meshCollider;
    private Collider gravityFieldCollider;

    [Header("KD-Tree Settings")]
    private KDTreeTriangle kdTree;
    private Vector3[] vertices;
    private Vector3[] normals;
    private List<Vector3[]> triangleList;

    [Header("Current Triangle Data")]
    private Triangle currentClosestTriangle;
    private Vector3 currentTriangleCenter;
    private HashSet<Triangle> currentNeighbors = new HashSet<Triangle>();

    [Header("Distance Thresholds")]
    [SerializeField] private float thresholdDistance = 0.1f;
    [SerializeField] private float neighborThresholdDistance = 0.1f;

    [Header("Gravity Direction")]
    private Vector3 gravityDirection = Vector3.zero;
    private Vector3 previousGravityDirection = Vector3.zero;

    [Header("Smoothing Settings")]
    [SerializeField] private float smoothingFactor = 10f;

    [Header("Gravity Delay")]
    public float gravityDelay = 0f; // Verzögerung in Sekunden
    private float delayTimer = 0f;

    public bool IsDelayActive => delayTimer > 0f;

    [Header("Directional Gravity Settings")]
    [SerializeField] private GravityDirection gravityDirectionType = GravityDirection.Down;




    void Awake()
    {
        gravityFieldCollider = GetComponent<Collider>();
        gravityFieldRadius = gravityFieldCollider.bounds.extents.magnitude;

        if (meshCollider != null)
        {
            Mesh mesh = meshCollider.sharedMesh;
            vertices = mesh.vertices;
            normals = mesh.normals;

            int[] triangles = mesh.triangles;
            triangleList = new List<Vector3[]>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3[] triangle = new Vector3[3];
                triangle[0] = meshCollider.transform.TransformPoint(vertices[triangles[i]]); // In Weltkoordinaten transformieren
                triangle[1] = meshCollider.transform.TransformPoint(vertices[triangles[i + 1]]);
                triangle[2] = meshCollider.transform.TransformPoint(vertices[triangles[i + 2]]);
                triangleList.Add(triangle);
            }

            if (gravityFieldType == GravityFieldType.HighPolyMeshKDTree)
            {
                kdTree = new KDTreeTriangle(triangleList.ToArray(), false); // Nachbarn nicht berechnen
            }
            else
            {
                kdTree = new KDTreeTriangle(triangleList.ToArray(), true); // Nachbarn berechnen
            }


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

    public void ResetDelay()
    {
        delayTimer = gravityDelay;
    }

    public void UpdateDelayTimer(float deltaTime)
    {
        if (delayTimer > 0f)
        {
            delayTimer -= deltaTime;
        }
    }
    public Vector3 CalculateGravityDirection(Vector3 playerPosition)
    {
        switch (gravityFieldType)
        {
            case GravityFieldType.Centerpoint:
                return (transform.position - playerPosition).normalized * gravityStrength;
            case GravityFieldType.TransformOneDirection:
                return transform.up * -1 * gravityStrength;
            case GravityFieldType.OneDirection:
                return GetDirectionalGravity() * gravityStrength;
            case GravityFieldType.CenterpointInverse:
                return (playerPosition - transform.position).normalized * gravityStrength;
            case GravityFieldType.SimpleMesh:
                return simpleCollider != null ? CalculateSimpleMeshBasedGravity(playerPosition) : LogErrorAndReturnZero("SimpleCollider is not set for SimpleMeshBased gravity field");
            case GravityFieldType.LowPolyMeshKDTree:
                return meshCollider != null ? GetInterpolatedGravityDirectionFromLowPolyMesh(playerPosition) : LogErrorAndReturnZero("MeshCollider is not set for MeshBasedKDTree gravity field");
            case GravityFieldType.HighPolyMeshKDTree:
                return meshCollider != null ? CalculateMeshBasedGravityHighPoly(playerPosition) : LogErrorAndReturnZero("MeshCollider is not set for MeshBasedKDTree gravity field");
            default:
                return (transform.position - playerPosition).normalized;
        }
    }

    private Vector3 GetDirectionalGravity()
    {
        switch (gravityDirectionType)
        {
            case GravityDirection.Up:
                return Vector3.up;
            case GravityDirection.Left:
                return Vector3.left;
            case GravityDirection.Right:
                return Vector3.right;
            case GravityDirection.Forward:
                return Vector3.forward;
            case GravityDirection.Backward:
                return Vector3.back;
            case GravityDirection.Down:
            default:
                return Vector3.down;
        }
    }

    private Vector3 LogErrorAndReturnZero(string message)
    {
        Debug.LogError(message);
        return Vector3.zero;
    }

    private Vector3 CalculateSimpleMeshBasedGravity(Vector3 playerPosition)
    {
        Vector3 closestPoint = simpleCollider.ClosestPoint(playerPosition);
        //Debug.Log("ClosestPoint ist: " + closestPoint);
        return (closestPoint - playerPosition).normalized * gravityStrength;
    }


    private Vector3 GetInterpolatedGravityDirectionFromLowPolyMesh(Vector3 playerPosition)
    {
        // Überprüfung, ob der Spieler eine bestimmte Distanz vom Mittelpunkt des aktuellen Dreiecks überschreitet
        bool needsUpdate = currentClosestTriangle == null ||
                           Vector3.Distance(playerPosition, currentTriangleCenter) > thresholdDistance;

        if (!needsUpdate && currentClosestTriangle != null)
        {
            needsUpdate = currentClosestTriangle.neighbors.Any(neighbor =>
                Vector3.Distance(playerPosition, neighbor.GetCenter()) < neighborThresholdDistance);
        }

        if (needsUpdate)
        {
            currentClosestTriangle = kdTree.FindNearestTriangleNode(playerPosition);
            if (currentClosestTriangle != null)
            {
                currentTriangleCenter = currentClosestTriangle.GetCenter();
            }
        }

        // Update der Nachbarn, bevor die Berechnungen fortfährt
        UpdateNeighbors();

        if (currentClosestTriangle != null)
        {
            Vector3 interpolatedGravity = Vector3.zero;
            float totalWeight = 0.0f;

            foreach (var neighbor in currentNeighbors)
            {
                Vector3 neighborCenter = neighbor.GetCenter();
                Vector3 normal = neighbor.normal;

                float distance = Vector3.Distance(playerPosition, neighborCenter);
                float weight = 1f / (distance + 0.001f); // Direkte Gewichtung

                interpolatedGravity += normal * weight;
                totalWeight += weight;
            }

            // Normale interpolieren und Richtung berechnen
            if (totalWeight > 0)
            {
                interpolatedGravity /= totalWeight;
            }
            else
            {
                Debug.LogWarning("Total weight is zero, fallback to current triangle normal");
                interpolatedGravity = currentClosestTriangle.normal;
            }

            // Entgegengesetzte Richtung für die Schwerkraft
            Vector3 targetGravityDirection = -interpolatedGravity.normalized * gravityStrength;

            // Initialisierung von previousGravityDirection nur einmal
            if (previousGravityDirection == Vector3.zero)
            {
                previousGravityDirection = targetGravityDirection;
            }

            // Glätte den Übergang zwischen der vorherigen und der neuen Richtung
            gravityDirection = Vector3.Lerp(previousGravityDirection, targetGravityDirection, Time.deltaTime * smoothingFactor);

            // Speichere die aktuelle Richtung für den nächsten Frame
            previousGravityDirection = gravityDirection;

            // Debug: Visualisiere interpolierte Richtung
            //Debug.DrawLine(playerPosition, playerPosition + gravityDirection, Color.green);

            return gravityDirection;
        }
        else
        {
            Debug.LogError("No closest triangle found in KDTree");
            return Vector3.zero;
        }
    }

    private Vector3 CalculateMeshBasedGravityHighPoly(Vector3 playerPosition)
    {
        // Finde das nächste Dreieck 
        Triangle closestTriangle = kdTree.FindNearestTriangleNode(playerPosition);

        //Interpoliere die Normalen
        return -closestTriangle.normal * gravityStrength;
    }

    // private Vector3 CalculateTriangleBasedGravity(Vector3 playerPosition)
    // {
    //     // Finde das nächste Dreieck
    //     Triangle closestNode = kdTree.FindNearestTriangleNode(playerPosition);
    //     Vector3[] closestTriangle = closestNode.vertices;

    //     // Berechne die baryzentrischen Koordinaten des Spielerpunktes
    //     Vector3 baryCoords = kdTree.CalculateBarycentricCoordinates(playerPosition, closestTriangle[0], closestTriangle[1], closestTriangle[2]);

    //     // Interpoliere die Normalen der Dreiecke
    //     Vector3 interpolatedNormal = closestNode.normal * baryCoords.x;

    //     // Berücksichtige die benachbarten Dreiecke
    //     foreach (Triangle neighbor in closestNode.neighbors)
    //     {
    //         Vector3 neighborBaryCoords = kdTree.CalculateBarycentricCoordinates(playerPosition, neighbor.vertices[0], neighbor.vertices[1], neighbor.vertices[2]);
    //         interpolatedNormal = Vector3.Lerp(interpolatedNormal, neighbor.normal, neighborBaryCoords.x);
    //     }

    //     interpolatedNormal = interpolatedNormal.normalized;

    //     return -interpolatedNormal * gravityStrength;
    // }

    // // Hilfsfunktion, um den nächstgelegenen Punkt auf einer Linie zu finden
    // public float DistanceToEdge(Vector3 point, Vector3 edgeStart, Vector3 edgeEnd)
    // {
    //     Vector3 edge = edgeEnd - edgeStart;
    //     Vector3 pointToStart = point - edgeStart;

    //     float edgeLengthSquared = edge.sqrMagnitude;
    //     if (edgeLengthSquared == 0.0f)
    //     {
    //         return pointToStart.magnitude;
    //     }

    //     float t = Vector3.Dot(pointToStart, edge) / edgeLengthSquared;
    //     t = Mathf.Clamp01(t);

    //     Vector3 projection = edgeStart + t * edge;
    //     return (point - projection).magnitude;
    // }

    // private Vector3 InterpolateNormalsAtEdge(Vector3 playerPosition, Triangle triangleA, Triangle triangleB, Vector3 edgeStart, Vector3 edgeEnd, float gravityStrength)
    // {
    //     // Berechne die Position des Spielers relativ zur Kante
    //     float t = Vector3.Dot(playerPosition - edgeStart, edgeEnd - edgeStart) / (edgeEnd - edgeStart).sqrMagnitude;
    //     t = Mathf.Clamp01(t); // Stelle sicher, dass t im Bereich [0, 1] liegt

    //     // Interpoliere die Normalen der beiden Dreiecke
    //     Vector3 interpolatedNormal = Vector3.Lerp(triangleA.normal, triangleB.normal, t).normalized;

    //     return -interpolatedNormal * gravityStrength;
    // }

    private void UpdateNeighbors()
    {
        HashSet<Triangle> newNeighbors = new HashSet<Triangle>(currentClosestTriangle.neighbors);

        // Erstellung einer Kopie der aktuellen Nachbarn, um sie zu iterieren
        var currentNeighborsCopy = currentNeighbors.ToList();

        foreach (var neighbor in currentNeighborsCopy)
        {
            // Überprüfung, ob der Nachbar noch zu den aktuellen Nachbarn gehört
            if (!newNeighbors.Contains(neighbor))
            {
                // Entferne Nachbarn, die nicht mehr relevant sind
                currentNeighbors.Remove(neighbor);
            }
        }

        // Neue Nachbarn werden hinzugefügt
        foreach (var neighbor in newNeighbors)
        {
            if (!currentNeighbors.Contains(neighbor))
            {
                currentNeighbors.Add(neighbor);
            }
        }
    }
    public float GravityStrength => gravityStrength;
    public float GravityFieldMass => gravityFieldMass;
    public float GravityFieldRadius => gravityFieldRadius;
    public float GravityDelay => gravityDelay;
    public GravityFieldType GravityFieldType => gravityFieldType;
    public int Priority => priority;
}

public enum GravityFieldType
{
    Centerpoint,
    TransformOneDirection,
    OneDirection,
    CenterpointInverse,
    SimpleMesh,
    LowPolyMeshKDTree,
    HighPolyMeshKDTree,
    CalculateInterpolatedGravityWithNeighbors
}

public enum GravityDirection
{
    Down,
    Up,
    Left,
    Right,
    Forward,
    Backward
}

//DER CODE HAT FUNKTIONIERT, ABER ES GAB PROBLEME MIT DER INTERPOLATION
//Definiere eine Schwellenentfernung für die Interpolation
// float thresholdDistance = 0.1f; // Passe diesen Wert nach Bedarf an

// //Finde das nächste Dreieck
// Triangle closestTriangle = kdTree.FindNearestTriangleNode(playerPosition);
// List<Triangle> neighbors = closestTriangle.neighbors;

// Debug.DrawLine(closestTriangle.GetCenter(), closestTriangle.GetCenter() + closestTriangle.normal, Color.blue, 1f);

// //Distanzen des Spielers zu den Nachbarn und deren Kanten
// float minEdgeDistance = float.MaxValue;
// Triangle closestNeighbor = null;
// Vector3 closestEdgeStart = Vector3.zero;
// Vector3 closestEdgeEnd = Vector3.zero;

// foreach (Triangle neighbor in neighbors)
// {
//     Debug.DrawLine(neighbor.GetCenter(), neighbor.GetCenter() + neighbor.normal, Color.red, 1f);

//     for (int i = 0; i < 3; i++)
//     {
//         Vector3 edgeStart = neighbor.vertices[i];
//         Vector3 edgeEnd = neighbor.vertices[(i + 1) % 3];
//         float edgeDistance = DistanceToEdge(playerPosition, edgeStart, edgeEnd);
//         if (edgeDistance < minEdgeDistance)
//         {
//             minEdgeDistance = edgeDistance;
//             closestNeighbor = neighbor;
//             closestEdgeStart = edgeStart;
//             closestEdgeEnd = edgeEnd;
//         }
//     }
// }

// //Zeichne eine Linie vom Spieler zur nächstgelegenen Kante
// if (closestNeighbor != null)
// {
//     Vector3 closestPointOnEdge = ClosestPointOnLineSegment(playerPosition, closestEdgeStart, closestEdgeEnd);
//     Debug.DrawLine(playerPosition, closestPointOnEdge, Color.green, 1f);
// }

// //Interpolation der Normalen der Dreiecke mit Slerp
// Vector3 interpolatedNormal;
// if (closestNeighbor != null && minEdgeDistance < thresholdDistance)
// {
//     Debug.DrawLine(closestNeighbor.GetCenter(), closestNeighbor.GetCenter() + closestNeighbor.normal, Color.yellow, 1f);
//     interpolatedNormal = Vector3.Slerp(closestTriangle.normal, closestNeighbor.normal, minEdgeDistance / thresholdDistance);
// }
// else
// {
//     interpolatedNormal = closestTriangle.normal;
// }

// return -interpolatedNormal * gravityStrength;

// // Wenn der Spieler nicht nahe einer Kante ist, nutze die Normale des aktuellen Dreiecks
// return -closestTriangle.normal * gravityStrength;

// Definiere eine Schwellenentfernung für die Interpolation
//     float thresholdDistance = 0.1f; // Passe diesen Wert nach Bedarf an

// Finde das nächste Dreieck
//     Triangle closestTriangle = kdTree.FindNearestTriangleNode(playerPosition);
// List<Triangle> neighbors = closestTriangle.neighbors;

// Debug.DrawLine(closestTriangle.CalculateCenter(), closestTriangle.CalculateCenter() + closestTriangle.normal, Color.blue, 1f);

// Distanzen des Spielers zu den Nachbarn und deren Kanten
//     float minEdgeDistance = float.MaxValue;
// Triangle closestNeighbor = null;
// Vector3 closestEdgeStart = Vector3.zero;
// Vector3 closestEdgeEnd = Vector3.zero;

// foreach (Triangle neighbor in neighbors)
// {
//     Debug.DrawLine(neighbor.CalculateCenter(), neighbor.CalculateCenter() + neighbor.normal, Color.red, 1f);

//     for (int i = 0; i < 3; i++)
//     {
//         Vector3 edgeStart = neighbor.vertices[i];
//         Vector3 edgeEnd = neighbor.vertices[(i + 1) % 3];
//         float edgeDistance = DistanceToEdge(playerPosition, edgeStart, edgeEnd);
//         if (edgeDistance < minEdgeDistance)
//         {
//             minEdgeDistance = edgeDistance;
//             closestNeighbor = neighbor;
//             closestEdgeStart = edgeStart;
//             closestEdgeEnd = edgeEnd;
//         }
//     }
// }

// Zeichne eine Linie vom Spieler zur nächstgelegenen Kante
//     if (closestNeighbor != null)
// {
//     Vector3 closestPointOnEdge = ClosestPointOnLineSegment(playerPosition, closestEdgeStart, closestEdgeEnd);
//     Debug.DrawLine(playerPosition, closestPointOnEdge, Color.green, 1f);
// }

// Interpolation der Normalen der Dreiecke mit Slerp
// Vector3 interpolatedNormal;
// if (closestNeighbor != null && minEdgeDistance < thresholdDistance)
// {
//     Debug.DrawLine(closestNeighbor.CalculateCenter(), closestNeighbor.CalculateCenter() + closestNeighbor.normal, Color.yellow, 1f);
//     interpolatedNormal = Vector3.Slerp(closestTriangle.normal, closestNeighbor.normal, minEdgeDistance / thresholdDistance);
// }
// else
// {
//     interpolatedNormal = closestTriangle.normal;
// }

// return -interpolatedNormal * gravityStrength;

// // Wenn der Spieler nicht nahe einer Kante ist, nutze die Normale des aktuellen Dreiecks
// return -closestTriangle.normal * gravityStrength;


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
