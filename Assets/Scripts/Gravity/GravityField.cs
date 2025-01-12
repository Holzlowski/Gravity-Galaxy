using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    [SerializeField] private GravityFieldType gravityFieldType;
    [SerializeField] private float gravityStrength = 9.81f;
    [SerializeField] private float gravityFieldMass = 10f;
    [SerializeField] private int priority = 0;
    private float gravityFieldRadius;
    [SerializeField] float gravityDelay = 0f; // Verzögerung in Sekunden
    private float delayTimer = 0f;
    public bool IsDelayActive => delayTimer > 0f;

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
    // private Triangle currentClosestTriangle;
    // private Vector3 currentTriangleCenter;
    private HashSet<Triangle> currentNeighbors = new HashSet<Triangle>();

    [Header("Distance Thresholds")]
    [SerializeField] private float thresholdDistance = 0.1f;
    [SerializeField] private float neighborThresholdDistance = 0.1f;

    [Header("Gravity Direction")]
    private Vector3 gravityDirection = Vector3.zero;
    private Vector3 previousGravityDirection = Vector3.zero;

    [Header("Smoothing Settings")]
    [SerializeField] private float smoothingFactor = 10f;



    [Header("Directional Gravity Settings")]
    [SerializeField] private GravityDirection gravityDirectionType = GravityDirection.Down;



    void Awake()
    {

        Physics.gravity = new Vector3(0, -15.0f, 0); // Erhöhte Gravitationskraft

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
    public Vector3 CalculateGravityDirection(Vector3 playerPosition, GravityState gravityState)
    {
        switch (gravityFieldType)
        {
            case GravityFieldType.Centerpoint:
                return (transform.position - playerPosition).normalized;
            case GravityFieldType.TransformOneDirection:
                return GetTransformDirectionalGravity();
            case GravityFieldType.OneDirection:
                return GetDirectionalGravity();
            case GravityFieldType.CenterpointInverse:
                return (playerPosition - transform.position).normalized;
            case GravityFieldType.SimpleMesh:
                return simpleCollider != null ? CalculateSimpleMeshBasedGravity(playerPosition) : LogErrorAndReturnZero("SimpleCollider is not set for SimpleMeshBased gravity field");
            case GravityFieldType.LowPolyMeshKDTree:
                return meshCollider != null ? GetInterpolatedGravityDirectionFromLowPolyMesh(playerPosition, gravityState) : LogErrorAndReturnZero("MeshCollider is not set for MeshBasedKDTree gravity field");
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

    private Vector3 GetTransformDirectionalGravity()
    {
        switch (gravityDirectionType)
        {
            case GravityDirection.Up:
                return transform.up;
            case GravityDirection.Down:
                return -transform.up;
            case GravityDirection.Left:
                return -transform.right;
            case GravityDirection.Right:
                return transform.right;
            case GravityDirection.Forward:
                return transform.forward;
            case GravityDirection.Backward:
                return -transform.forward;
            default:
                return -transform.up;
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
        return (closestPoint - playerPosition).normalized;
    }


    private Vector3 GetInterpolatedGravityDirectionFromLowPolyMesh(Vector3 playerPosition, GravityState state)
    {
        Triangle newClosestTriangle = kdTree.FindNearestTriangleNode(playerPosition);

        if (newClosestTriangle == null)
        {
            Debug.LogError("No closest triangle found in KDTree");
            return Vector3.zero;
        }

        bool needsUpdate = state.currentClosestTriangle == null ||
                           Vector3.Distance(playerPosition, state.currentTriangleCenter) > thresholdDistance;

        if (!needsUpdate)
        {
            foreach (var neighbor in state.currentClosestTriangle.neighbors)
            {
                if (Vector3.Distance(playerPosition, neighbor.GetCenter()) < neighborThresholdDistance)
                {
                    needsUpdate = true;
                    break;
                }
            }
        }

        if (needsUpdate)
        {
            state.currentClosestTriangle = newClosestTriangle;
            state.currentTriangleCenter = state.currentClosestTriangle.GetCenter();
            UpdateNeighbors(state);
        }

        Vector3 interpolatedGravity = Vector3.zero;
        float totalWeight = 0.0f;

        foreach (var neighbor in state.currentNeighbors)
        {
            float weight = 1f / (Vector3.Distance(playerPosition, neighbor.GetCenter()) + 0.001f);
            interpolatedGravity += neighbor.normal * weight;
            totalWeight += weight;
        }

        if (totalWeight > 0)
        {
            interpolatedGravity /= totalWeight;
        }
        else
        {
            Debug.LogWarning("Total weight is zero, fallback to current triangle normal");
            interpolatedGravity = state.currentClosestTriangle.normal;
        }

        Vector3 targetGravityDirection = -interpolatedGravity.normalized * gravityStrength;

        if (state.previousGravityDirection == Vector3.zero)
        {
            state.previousGravityDirection = targetGravityDirection;
        }

        Vector3 gravityDirection = Vector3.Lerp(state.previousGravityDirection,
        targetGravityDirection, Time.deltaTime * smoothingFactor);

        state.previousGravityDirection = gravityDirection;

        return gravityDirection;
    }

    private Vector3 CalculateMeshBasedGravityHighPoly(Vector3 playerPosition)
    {
        // Finde das nächste Dreieck 
        Triangle closestTriangle = kdTree.FindNearestTriangleNode(playerPosition);

        //Interpoliere die Normalen
        return -closestTriangle.normal;
    }

    private void UpdateNeighbors(GravityState state)
    {
        HashSet<Triangle> newNeighbors = new HashSet<Triangle>(state.currentClosestTriangle.neighbors);

        foreach (var neighbor in state.currentNeighbors.ToList())
        {
            if (!newNeighbors.Contains(neighbor))
            {
                state.currentNeighbors.Remove(neighbor);
            }
        }

        foreach (var neighbor in newNeighbors)
        {
            if (!state.currentNeighbors.Contains(neighbor))
            {
                state.currentNeighbors.Add(neighbor);
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
    HighPolyMeshKDTree
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
