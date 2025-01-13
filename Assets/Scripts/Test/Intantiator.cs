using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intantiator : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToInstantiate; // Das Objekt, das instanziiert werden soll

    [SerializeField]
    private Transform spawnCenter; // Das Zentrum des Kreises, innerhalb dessen die Objekte instanziiert werden sollen

    [SerializeField]
    private int numberOfObjects = 10; // Anzahl der zu instanziierenden Objekte

    [SerializeField]
    private float radius = 5f; // Radius des Kreises, innerhalb dessen die Objekte instanziiert werden sollen

    [SerializeField]
    private float spawnInterval = 1f; // Zeitintervall zwischen den Spawns

    [SerializeField]
    private bool spawnSequentially = false; // Bool, um zwischen den Modi zu wechseln

    void Start()
    {
        if (spawnSequentially)
        {
            StartCoroutine(SpawnObjectsSequentially());
        }
        else
        {
            SpawnAllObjects();
        }
    }

    private void SpawnAllObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            InstantiateObject();
        }
    }

    private IEnumerator SpawnObjectsSequentially()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            InstantiateObject();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void InstantiateObject()
    {
        // Zufällige Position innerhalb des Kreises berechnen
        Vector3 randomPosition = GetRandomPositionWithinCircle(spawnCenter.position, radius);

        // Objekt instanziieren
        Instantiate(objectToInstantiate, randomPosition, Quaternion.identity);
    }

    private Vector3 GetRandomPositionWithinCircle(Vector3 center, float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, radius);

        float x = center.x + Mathf.Cos(angle) * distance;
        float z = center.z + Mathf.Sin(angle) * distance;

        return new Vector3(x, center.y, z);
    }
}
