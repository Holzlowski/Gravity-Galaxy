using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHead : MonoBehaviour
{
    // Dieser Code wird ausgeführt, wenn der Trigger-Collider den Spieler berührt.
     private void OnTriggerEnter(Collider other)
    {
        // Überprüfe, ob der kollidierende Spieler ist
        if (other.CompareTag("Player"))
        {
            // Hole die GroundDetection-Komponente vom Spieler
            GroundDetection groundDetection = other.GetComponent<GroundDetection>();
            // Prüfe, ob der Spieler nicht geerdet ist
            if (groundDetection != null && !groundDetection.IsGrounded)
            {
                Destroy(gameObject);
            }
        }
    }
}
