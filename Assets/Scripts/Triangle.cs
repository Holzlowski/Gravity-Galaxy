using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Triangle
{

    public Vector3[] vertices; // Das Dreieck im 3D-Raum, das der Knoten repräsentiert
    public Vector3 normal; // Die Normale des Dreiecks
    public List<Triangle> neighbors; // Liste der Nachbardreiecke
    public Triangle left;
    public Triangle right;



    public Triangle(Vector3[] vertices, Vector3 normal)
    {
        this.vertices = vertices;
        this.normal = normal;
        this.neighbors = new List<Triangle>();
    }

    public Vector3 GetCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
        }

        return center /= 3;
    }
}