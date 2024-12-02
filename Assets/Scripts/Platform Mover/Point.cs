using UnityEngine;

public class Point : MonoBehaviour
{
    [Tooltip("Zeit, die an diesem Punkt gewartet wird.")]
    public float waitTime = 0f;

    [Tooltip("Optionales Zielobjekt, das sich an diesem Punkt drehen soll.")]
    public Transform targetObject;
}



