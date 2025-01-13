using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intantiator : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToInstantiate; // Das Objekt, das instanziiert werden soll

    [SerializeField]
    private MeshFilter spawnMeshFilter; // Das Mesh, auf dem die Objekte instanziiert werden sollen

    [SerializeField]
    private int numberOfObjects = 10; // Anzahl der zu instanziierenden Objekte

    private Mesh spawnMesh;

    void Start()
    {
        if (spawnMeshFilter != null)
        {
            spawnMesh = spawnMeshFilter.mesh;
        }
        else
        {
            Debug.LogError("No MeshFilter found on the spawnMeshFilter object.");
            return;
        }

        SpawnAllObjects();
    }

    private void SpawnAllObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            InstantiateObject();
        }
    }

    private void InstantiateObject()
    {
        // Zufällige Position auf dem Mesh berechnen
        Vector3 randomPosition = GetRandomPositionOnMesh(spawnMesh);

        // Objekt instanziieren
        Instantiate(objectToInstantiate, randomPosition, Quaternion.identity);
    }

    private Vector3 GetRandomPositionOnMesh(Mesh mesh)
    {
        // Wähle ein zufälliges Dreieck aus dem Mesh
        int triangleIndex = Random.Range(0, mesh.triangles.Length / 3) * 3;

        // Hole die Eckpunkte des Dreiecks
        Vector3 vertex1 = mesh.vertices[mesh.triangles[triangleIndex]];
        Vector3 vertex2 = mesh.vertices[mesh.triangles[triangleIndex + 1]];
        Vector3 vertex3 = mesh.vertices[mesh.triangles[triangleIndex + 2]];

        // Berechne eine zufällige Position innerhalb des Dreiecks
        Vector3 randomPosition = GetRandomPointInTriangle(vertex1, vertex2, vertex3);

        // Transformiere die Position in den Weltkoordinatenraum
        return spawnMeshFilter.transform.TransformPoint(randomPosition);
    }

    private Vector3 GetRandomPointInTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float a = Random.value;
        float b = Random.value;

        // Stelle sicher, dass a + b <= 1
        if (a + b > 1)
        {
            a = 1 - a;
            b = 1 - b;
        }

        // Berechne die zufällige Position innerhalb des Dreiecks
        return v1 + a * (v2 - v1) + b * (v3 - v1);
    }
}