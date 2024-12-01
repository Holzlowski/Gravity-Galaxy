using System.Collections.Generic;
using UnityEngine;

public class PointNetwork : MonoBehaviour
{
    public List<Point> points = new List<Point>();
    public bool loop = false; // Ob der erste und letzte Punkt verbunden sind

    void Awake()
    {
        // Liste der Punkte aus den Kindobjekten erstellen
        points.Clear();
        foreach (Transform child in transform)
        {
            Point point = child.GetComponent<Point>();
            if (point != null)
            {
                points.Add(point);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (points.Count > 1)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawConnection(points[i], points[i + 1]);
            }

            if (loop)
            {
                DrawConnection(points[points.Count - 1], points[0]);
            }
        }
    }

    void DrawConnection(Point a, Point b)
    {

        // Zeichne eine gerade Linie zwischen den Punkten
        Gizmos.color = Color.green;
        Gizmos.DrawLine(a.transform.position, b.transform.position);

    }
}