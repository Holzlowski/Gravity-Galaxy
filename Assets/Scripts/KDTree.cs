using System.Collections.Generic;

using UnityEngine;

public class KDTree
{
    // Innere Klasse, um Knoten des Baums darzustellen
    private class Node
    {
        public Vector3 point; // Der Punkt im 3D-Raum, den der Knoten repräsentiert
        public Node left;     // Linker Teilbaum (niedrigere Werte entlang der aktuellen Achse)
        public Node right;    // Rechter Teilbaum (höhere Werte entlang der aktuellen Achse)
    }

    private Node root; // Wurzel des KD-Baums

    // Konstruktor, der den Baum aus einem Array von 3D-Punkten erstellt
    public KDTree(Vector3[] points)
    {
        root = BuildTree(points, 0); // Beginne mit der Tiefe 0
    }

    // Rekursive Methode zum Erstellen des KD-Baums
    private Node BuildTree(Vector3[] points, int depth)
    {
        // Basisfall: Wenn keine Punkte mehr übrig sind, gibt es keinen Knoten
        if (points.Length == 0)
            return null;

        // Bestimme die Achse basierend auf der Tiefe (zyklisch zwischen 0, 1 und 2: x, y, z)
        int axis = depth % 3;

        // Sortiere die Punkte entlang der aktuellen Achse
        System.Array.Sort(points, (a, b) => a[axis].CompareTo(b[axis]));

        // Finde den Median, um den Baum auszugleichen
        int median = points.Length / 2;

        // Erstelle einen neuen Knoten mit dem Median-Punkt
        Node node = new Node
        {
            point = points[median],
            // Rekursiv die linken und rechten Teilbäume erstellen
            left = BuildTree(points[..median], depth + 1),            // Linke Hälfte der Punkte
            right = BuildTree(points[(median + 1)..], depth + 1)     // Rechte Hälfte der Punkte
        };

        return node; // Rückgabe des erstellten Knotens
    }

    // Findet den nächsten Punkt im Baum, der einem Zielpunkt am nächsten liegt
    public Vector3 FindNearest(Vector3 target)
    {
        // Beginne bei der Wurzel und starte die Suche
        return FindNearest(root, target, 0).point;
    }

    // Rekursive Methode, um den nächsten Punkt zu finden
    private Node FindNearest(Node node, Vector3 target, int depth)
    {
        // Basisfall: Wenn der Knoten null ist, gibt es keine nähere Option
        if (node == null)
            return null;

        // Bestimme die aktuelle Achse basierend auf der Tiefe
        int axis = depth % 3;

        // Entscheide, welchen Teilbaum du zuerst durchsuchen möchtest
        Node nextBranch = (target[axis] < node.point[axis]) ? node.left : node.right;
        Node otherBranch = (target[axis] < node.point[axis]) ? node.right : node.left;

        // Suche rekursiv im bevorzugten Teilbaum
        Node best = CloserDistance(target, FindNearest(nextBranch, target, depth + 1), node);

        // Prüfe, ob es im anderen Teilbaum möglicherweise einen näheren Punkt gibt
        if (otherBranch != null && Mathf.Abs(target[axis] - node.point[axis]) < Vector3.Distance(target, best.point))
        {
            // Falls ja, überprüfe auch diesen Teilbaum
            best = CloserDistance(target, FindNearest(otherBranch, target, depth + 1), best);
        }

        return best; // Gib den besten gefundenen Knoten zurück
    }

    // Hilfsmethode, um den näheren von zwei Knoten zu bestimmen
    private Node CloserDistance(Vector3 target, Node a, Node b)
    {
        // Wenn einer der Knoten null ist, gib den anderen zurück
        if (a == null) return b;
        if (b == null) return a;

        // Vergleiche die Distanzen beider Knoten zum Zielpunkt und gib den näheren zurück
        return (Vector3.Distance(target, a.point) < Vector3.Distance(target, b.point)) ? a : b;
    }

    // public List<Vector3> FindKNearest(Vector3 target, int k)
    // {
    //     // Verwende SortedDictionary, um die k nächsten Punkte effizient zu speichern
    //     var nearestNeighbors = new SortedDictionary<float, List<Node>>();
    //     FindKNearest(root, target, k, 0, nearestNeighbors);

    //     // Extrahiere die k nächstgelegenen Punkte aus dem SortedDictionary
    //     var result = new List<Vector3>();
    //     foreach (var neighborList in nearestNeighbors.Values)
    //     {
    //         foreach (var neighbor in neighborList)
    //         {
    //             result.Add(neighbor.point);
    //         }
    //     }

    //     return result;
    // }

    // private void FindKNearest(Node node, Vector3 target, int k, int depth, SortedDictionary<float, List<Node>> nearestNeighbors)
    // {
    //     if (node == null)
    //         return;

    //     int axis = depth % 3;

    //     float distance = Vector3.Distance(node.point, target);

    //     // Füge den aktuellen Punkt in das SortedDictionary ein (verwende eine Liste, um gleiche Distanzen zu handhaben)
    //     if (nearestNeighbors.Count < k)
    //     {
    //         if (!nearestNeighbors.ContainsKey(distance))
    //         {
    //             nearestNeighbors[distance] = new List<Node>();
    //         }
    //         nearestNeighbors[distance].Add(node);
    //     }
    //     else
    //     {
    //         // Holen des maximalen Schlüssels durch den Enumerator
    //         var enumerator = nearestNeighbors.GetEnumerator();
    //         enumerator.MoveNext(); // Gehe zum ersten Eintrag
    //         float maxDistance = enumerator.Current.Key;

    //         // Jetzt gehe bis zum letzten Eintrag
    //         while (enumerator.MoveNext())
    //         {
    //             maxDistance = enumerator.Current.Key;
    //         }

    //         if (distance < maxDistance)
    //         {
    //             // Entferne den Punkt mit der größten Distanz (letztes Element)
    //             nearestNeighbors.Remove(maxDistance);

    //             if (!nearestNeighbors.ContainsKey(distance))
    //             {
    //                 nearestNeighbors[distance] = new List<Node>();
    //             }
    //             nearestNeighbors[distance].Add(node);
    //         }
    //     }

    //     // Rekursiv im bevorzugten Teilbaum suchen
    //     Node nextBranch = (target[axis] < node.point[axis]) ? node.left : node.right;
    //     Node otherBranch = (target[axis] < node.point[axis]) ? node.right : node.left;

    //     FindKNearest(nextBranch, target, k, depth + 1, nearestNeighbors);

    //     // Prüfe, ob der andere Teilbaum relevant ist
    //     var enumerator2 = nearestNeighbors.GetEnumerator();
    //     enumerator2.MoveNext();
    //     if (otherBranch != null && Mathf.Abs(target[axis] - node.point[axis]) < enumerator2.Current.Key)
    //     {
    //         FindKNearest(otherBranch, target, k, depth + 1, nearestNeighbors);
    //     }
    // }


}
