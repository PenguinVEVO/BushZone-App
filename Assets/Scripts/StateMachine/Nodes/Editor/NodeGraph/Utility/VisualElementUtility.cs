using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementUtility
{
    /// <summary>
    /// Walks up the hierarchy from the picked element to find two distinct types.
    /// </summary>
    public static void FindTarget<T1, T2>(VisualElement start, out T1 result1, 
        out T2 result2, int maxDepth = 20) where T1 : VisualElement where T2 : VisualElement
    {
        result1 = null;
        result2 = null;

        VisualElement current = start;
        int depth = 0;

        while (current != null && depth < maxDepth)
        {
            if (result1 == null && current is T1 t1) result1 = t1;
            if (result2 == null && current is T2 t2) result2 = t2;

            if (result1 != null && result2 != null)
                return;

            current = current.parent;
            depth++;
        }
    }

    public static void DrawDashedLine(Painter2D painter, List<Vector3> points, float dashLength, float gapLength)
    {
        bool drawingDash = true;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 start = new Vector2(points[i].x, points[i].y);
            Vector2 end = new Vector2(points[i + 1].x, points[i + 1].y);
            Vector2 dir = (end - start).normalized;
            float distance = Vector2.Distance(start, end);
            float traveled = 0f;
            Vector2 current = start;

            while (traveled < distance)
            {
                float segmentLength = drawingDash ? dashLength : gapLength;
                float nextStep = Mathf.Min(segmentLength, distance - traveled);
                Vector2 nextPoint = current + dir * nextStep;

                if (drawingDash)
                {
                    painter.BeginPath();
                    painter.MoveTo(current);
                    painter.LineTo(nextPoint);
                    painter.Stroke();
                }

                current = nextPoint;
                traveled += nextStep;
                drawingDash = !drawingDash;
            }
        }
    }

    /// <summary>
    /// Recursively traverses the VisualElement hierarchy and executes a callback for each element.
    /// </summary>
    public static void Traverse(VisualElement root, Action<VisualElement> action)
    {
        if (root == null || action == null)
            return;

        action(root);

        foreach (var child in root.Children())
        {
            Traverse(child, action);
        }
    }

    /// <summary>
    /// Finds the first element matching a class name.
    /// </summary>
    public static VisualElement FindByClass(VisualElement root, string className)
    {
        VisualElement result = null;
        Traverse(root, ve =>
        {
            if (ve.ClassListContains(className) && result == null)
                result = ve;
        });
        return result;
    }

    /// <summary>
    /// Finds the first element matching a name (e.g. "#editor-message-box").
    /// </summary>
    public static VisualElement FindByName(VisualElement root, string name)
    {
        VisualElement result = null;
        Traverse(root, ve =>
        {
            if (ve.name == name && result == null)
                result = ve;
        });
        return result;
    }

    /// <summary>
    /// Traverses the entire ancestor chain to find the first or last matching VisualElement of type T.
    /// </summary>
    public static T FindInAncestors<T>(VisualElement start, bool returnLast = false, 
        int maxDepth = int.MaxValue) where T : VisualElement
    {
        if (start == null) return null;

        VisualElement current = start;
        T lastMatch = null;
        int depth = 0;

        while (current.parent != null && depth++ < maxDepth)
        {
            current = current.parent;

            if (current is T match)
            {
                if (!returnLast)
                    return match;

                lastMatch = match;
            }
        }

        return lastMatch;
    }


/// <summary>
/// Traverses up to find the top-most root VisualElement (can also be bounded by depth).
/// </summary>
public static VisualElement GetRoot(VisualElement start, int maxDepth = int.MaxValue)
    {
        if (start == null) return null;

        VisualElement current = start;
        int depth = 0;

        while (current.parent != null && depth < maxDepth)
        {
            current = current.parent;
            depth++;
        }

        return current;
    }

    /// <summary>
    /// Logs the hierarchy tree structure (for debugging).
    /// </summary>
    public static void DebugHierarchy(VisualElement root, int depth = 0)
    {
        if (root == null) return;

        string indent = new string(' ', depth * 2);
        string classStr = string.Join(" ", root.GetClasses());
        Debug.Log($"{indent}{root.GetType().Name} #{root.name} .{classStr}");

        foreach (var child in root.Children())
        {
            DebugHierarchy(child, depth + 1);
        }
    }
}
