using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    [SerializeField]
    private Transform centerPoint;

    [SerializeField]
    private float rotationSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        RotatePlanet();
    }

    private void RotatePlanet()
    {
        transform.RotateAround(centerPoint.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
