using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private float rotationSpeed = 1.0f;

    [SerializeField]
    private float smoothSpeed = 0.125f;

    [SerializeField]
    private Vector3 offset;

    void LateUpdate()
    {
        // Kamera um den Spieler rotieren lassen
        if (Input.GetKey(KeyCode.Q))
        {
            RotateCameraAroundTarget(-target.up);
        }
        if (Input.GetKey(KeyCode.E))
        {
            RotateCameraAroundTarget(target.up);
        }

        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    private void UpdateCameraPosition()
    {
        Vector3 desiredPosition = CalculateDesiredPosition();
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
        transform.position = smoothedPosition;
    }

    private Vector3 CalculateDesiredPosition()
    {
        return target.position + target.rotation * offset;
    }

    private void UpdateCameraRotation()
    {
        Quaternion targetRotation = CalculateTargetRotation();
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            smoothSpeed * Time.deltaTime
        );
    }

    private Quaternion CalculateTargetRotation()
    {
        return Quaternion.LookRotation(target.position - transform.position, target.up);
    }

    private void RotateCameraAroundTarget(Vector3 direction)
    {
        cameraTransform.RotateAround(target.position, direction, rotationSpeed * Time.deltaTime);
    }
}


// interessante Kameraführung xD
// else
// {
//     desiredPosition = Vector3.Scale(target.position, offset);
// }
