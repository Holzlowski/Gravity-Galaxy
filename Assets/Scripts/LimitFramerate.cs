using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFramerate : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60; // Set your desired frame rate here

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0; // Disable vSync
        Application.targetFrameRate = targetFrameRate; // Set the target frame rate
    }
}
