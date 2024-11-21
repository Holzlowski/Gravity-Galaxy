using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDTreeTriangle
{
    private class Node
    {
        public Vector3[] triangle; // Das Dreieck im 3D-Raum, das der Knoten repräsentiert
        public Node left;
        public Node right;
    }

    private Node root;

    public KDTreeTriangle(Vector3[][] triangles)
    {
        root = BuildTree(triangles, 0);
    }

    private Node BuildTree(Vector3[][] triangles, int depth)
    {
        if (triangles.Length == 0)
            return null;

        int axis = depth % 3;
        System.Array.Sort(triangles, (a, b) => a[0][axis].CompareTo(b[0][axis]));

        int median = triangles.Length / 2;
        Node node = new Node
        {
            triangle = triangles[median],
            left = BuildTree(triangles[..median], depth + 1),
            right = BuildTree(triangles[(median + 1)..], depth + 1)
        };

        return node;
    }

    public Vector3[] FindNearestTriangle(Vector3 target)
    {
        return FindNearestTriangle(root, target, 0).triangle;
    }

    private Node FindNearestTriangle(Node node, Vector3 target, int depth)
    {
        if (node == null)
            return null;

        int axis = depth % 3;
        Node nextBranch = (target[axis] < node.triangle[0][axis]) ? node.left : node.right;
        Node otherBranch = (target[axis] < node.triangle[0][axis]) ? node.right : node.left;

        Node best = CloserDistance(target, FindNearestTriangle(nextBranch, target, depth + 1), node);

        if (otherBranch != null && Mathf.Abs(target[axis] - node.triangle[0][axis]) < DistanceToTriangle(target, best.triangle))
        {
            best = CloserDistance(target, FindNearestTriangle(otherBranch, target, depth + 1), best);
        }

        return best;
    }

    private Node CloserDistance(Vector3 target, Node a, Node b)
    {
        if (a == null) return b;
        if (b == null) return a;
        return (DistanceToTriangle(target, a.triangle) < DistanceToTriangle(target, b.triangle)) ? a : b;
    }

    private float DistanceToTriangle(Vector3 point, Vector3[] triangle)
    {
        // Berechne die Entfernung vom Punkt zum Dreieck
        Vector3 closestPoint = ClosestPointOnTriangle(point, triangle[0], triangle[1], triangle[2]);
        return Vector3.Distance(point, closestPoint);
    }

    private Vector3 ClosestPointOnTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
    {
        // Berechne den nächstgelegenen Punkt auf dem Dreieck
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = point - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);

        if (d1 <= 0.0f && d2 <= 0.0f) return a;

        Vector3 bp = point - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);

        if (d3 >= 0.0f && d4 <= d3) return b;

        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
        {
            float v = d1 / (d1 - d3);
            return a + v * ab;
        }

        Vector3 cp = point - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);

        if (d6 >= 0.0f && d5 <= d6) return c;

        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
        {
            float w = d2 / (d2 - d6);
            return a + w * ac;
        }

        float va = d3 * d6 - d5 * d4;
        if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
        {
            float u = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + u * (c - b);
        }

        float denom = 1.0f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        return a + ab * v2 + ac * w2;
    }
}
