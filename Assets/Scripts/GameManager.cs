using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public GameObject player;
    public bool useNormals = false;
    public bool allowSpaceFlight = false;
    public bool switchGravityFieldBasedOnDistance = false;
    public bool useGravityLaw = false;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Falls keine Instanz gefunden wurde, wird im Log eine Warnung ausgegeben
                Debug.LogWarning("GameManager instance not found!");
            }
            return _instance;
        }
    }

    // Festlegen, ob die Instanz beim Szenenwechsel erhalten bleibt
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // Wenn eine andere Instanz vorhanden ist, diese zerstören
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject); // Das Singleton bleibt über Szenen hinweg
        }
    }
}
