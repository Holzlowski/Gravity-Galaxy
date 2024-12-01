using System.Collections.Generic;
using UnityEngine;

public class KDTreeTriangle
{
    private Triangle root;

    public KDTreeTriangle(Vector3[][] triangles, bool buildNeighbors = true)
    {
        root = BuildTree(triangles, 0);
        if (buildNeighbors)
        {
            BuildNeighbors(root, triangles);
        }
    }

    private Triangle BuildTree(Vector3[][] triangles, int depth)
    {
        if (triangles.Length == 0)
            return null;

        int axis = depth % 3;
        System.Array.Sort(triangles, (a, b) => a[0][axis].CompareTo(b[0][axis]));

        int median = triangles.Length / 2;
        Triangle node = new Triangle(triangles[median], CalculateTriangleNormal(triangles[median]))
        {
            neighbors = new List<Triangle>(),
            left = BuildTree(triangles[..median], depth + 1),
            right = BuildTree(triangles[(median + 1)..], depth + 1)
        };

        return node;
    }

    private void BuildNeighbors(Triangle node, Vector3[][] triangles)
    {
        if (node == null)
            return;

        foreach (var triangle in triangles)
        {
            if (triangle != node.vertices && AreTrianglesNeighbors(node.vertices, triangle))
            {
                Triangle neighborNode = FindNode(root, triangle);
                if (neighborNode != null)
                {
                    node.neighbors.Add(neighborNode);
                }
            }
        }

        BuildNeighbors(node.left, triangles);
        BuildNeighbors(node.right, triangles);
    }

    private Triangle FindNode(Triangle node, Vector3[] triangle)
    {
        if (node == null)
            return null;

        if (node.vertices == triangle)
            return node;

        Triangle foundNode = FindNode(node.left, triangle);
        if (foundNode == null)
        {
            foundNode = FindNode(node.right, triangle);
        }

        return foundNode;
    }

    private bool AreTrianglesNeighbors(Vector3[] triangle1, Vector3[] triangle2)
    {
        foreach (var vertex1 in triangle1)
        {
            foreach (var vertex2 in triangle2)
            {
                if (vertex1 == vertex2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // private bool AreTrianglesNeighbors(Vector3[] triangle1, Vector3[] triangle2)
    // {
    //     int sharedVertices = 0;
    //     foreach (var vertex1 in triangle1)
    //     {
    //         foreach (var vertex2 in triangle2)
    //         {
    //             if (vertex1 == vertex2)
    //             {
    //                 sharedVertices++;
    //             }
    //         }
    //     }
    //     return sharedVertices >= 2;
    // }

    public Triangle FindNearestTriangleNode(Vector3 target)
    {
        return FindNearestTriangle(root, target, 0);
    }

    private Triangle FindNearestTriangle(Triangle node, Vector3 target, int depth)
    {
        if (node == null)
            return null;

        int axis = depth % 3;
        Triangle nextBranch = (target[axis] < node.vertices[0][axis]) ? node.left : node.right;
        Triangle otherBranch = (target[axis] < node.vertices[0][axis]) ? node.right : node.left;

        Triangle best = CloserDistance(target, FindNearestTriangle(nextBranch, target, depth + 1), node);

        if (otherBranch != null && Mathf.Abs(target[axis] - node.vertices[0][axis]) < DistanceToTriangle(target, best.vertices))
        {
            best = CloserDistance(target, FindNearestTriangle(otherBranch, target, depth + 1), best);
        }

        return best;
    }

    private Triangle CloserDistance(Vector3 target, Triangle a, Triangle b)
    {
        if (a == null) return b;
        if (b == null) return a;
        return (DistanceToTriangle(target, a.vertices) < DistanceToTriangle(target, b.vertices)) ? a : b;
    }

    private float DistanceToTriangle(Vector3 point, Vector3[] triangle)
    {
        Vector3 triangleCenter = (triangle[0] + triangle[1] + triangle[2]) / 3.0f;
        return Vector3.Distance(point, triangleCenter);
    }

    public Vector3 CalculateBarycentricCoordinates(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = point - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;
        return new Vector3(u, v, w);
    }

    private Vector3 CalculateTriangleNormal(Vector3[] triangle)
    {
        Vector3 v0 = triangle[0];
        Vector3 v1 = triangle[1];
        Vector3 v2 = triangle[2];
        return Vector3.Cross(v1 - v0, v2 - v0).normalized;
    }
}