using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class SunController : MonoBehaviour
{
    [SerializeField]
    private Transform centerPoint;

    [SerializeField]
    private Transform lightSource;

    [SerializeField]
    private float sunRotationSpeed = 0.1f;

    [SerializeField]
    private float skyBoxRotationSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        lightSource.position = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        RotateSun();
        RotateSkybox();
    }

    private void RotateSun()
    {
        transform.RotateAround(
            centerPoint.position,
            -Vector3.up,
            sunRotationSpeed * Time.deltaTime
        );
        transform.LookAt(centerPoint);
        lightSource.LookAt(centerPoint);
    }

    private void RotateSkybox()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyBoxRotationSpeed);
    }
}
