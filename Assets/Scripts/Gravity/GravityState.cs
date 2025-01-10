using UnityEngine;
using System.Collections.Generic;

public class GravityState
{
    public Triangle currentClosestTriangle = null;
    public Vector3 currentTriangleCenter = Vector3.zero;
    public HashSet<Triangle> currentNeighbors = new HashSet<Triangle>();
    public Vector3 previousGravityDirection = Vector3.zero;
}