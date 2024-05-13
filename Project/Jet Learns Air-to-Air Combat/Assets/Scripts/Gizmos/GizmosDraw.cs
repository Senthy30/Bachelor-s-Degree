#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GizmosDraw {
    
    // Public methods ---------------------------------------------------------

    // Cone drawing methods --------------------------------------------------

    public static void DrawCone(Transform transform, float angle, float length, int numCirclesDivisions, Color color) {
        float lengthAtAngle = length * Mathf.Tan(angle * Mathf.Deg2Rad);
        float realLength = Mathf.Sqrt(length * length + lengthAtAngle * lengthAtAngle);

        DrawLineAtAngle(transform.position, transform.forward, transform.up, angle, realLength, color);
        DrawLineAtAngle(transform.position, transform.forward, transform.up, -angle, realLength, color);
        DrawLineAtAngle(transform.position, transform.forward, transform.right, angle, realLength, color);
        DrawLineAtAngle(transform.position, transform.forward, transform.right, -angle, realLength, color);

        Vector3 lastCircleCenter = transform.position + transform.forward * length;
        DrawCircle(lastCircleCenter, transform.forward, transform.right, length * Mathf.Tan(angle * Mathf.Deg2Rad), color);
        ++numCirclesDivisions;
        for (int i = 1; i <= numCirclesDivisions; i++) {
            float currentLength = length * i / numCirclesDivisions;
            float currentLengthAtAngle = currentLength * Mathf.Tan(angle * Mathf.Deg2Rad);
            Vector3 currentCircleCenter = transform.position + transform.forward * currentLength;
            DrawCircle(currentCircleCenter, transform.forward, transform.right, currentLengthAtAngle, color);
        }
    }

    // Circle drawing methods ------------------------------------------------

    public static void DrawCircle(Vector3 center, Vector3 forward, Vector3 from, float radius, Color color) {
        Handles.color = color;
        Handles.DrawWireArc(center, forward, from, 360, radius);
    }

    // Line drawing methods ---------------------------------------------------
    public static void DrawLineAtAngle(Vector3 start, Vector3 forward, Vector3 axis, float angle, float length, Color color) {
        Quaternion rayRotation = Quaternion.AngleAxis(angle, axis);
        Vector3 rayDirection = rayRotation * forward;
        Gizmos.DrawRay(start, rayDirection * length);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color) {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
    }

    // Private methods --------------------------------------------------------

}
#endif