#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class AerodynamicSurfaceConfigDrawer {

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    public static void AircraftPhysicsGizmos(AircraftPhysics phys, GizmoType gizmoType) {
        Vector3 weight = phys.GetComponent<Rigidbody>().mass * Physics.gravity;
        float scale = 1f;
        scale /= weight.magnitude;

        Gizmos.color = new Color(255, 127, 0);
        Vector3 com = phys.GetComponent<Rigidbody>().worldCenterOfMass;
        Gizmos.DrawWireSphere(com, 0.3f);
        DrawThinArrow(com, weight * scale, new Color(255, 127, 0), 0.4f, 3);

        Vector3 airspeed = new Vector3(0, Mathf.Sin(Mathf.Deg2Rad * 5f), -Mathf.Cos(Mathf.Deg2Rad * 5f)) * 50f;

        Vector3 center;
        Vector3 force;
        phys.CalculateCenterOfLift(out center, out force, phys.transform.TransformDirection(airspeed), 1.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, 0.3f);
        DrawThinArrow(center, force * scale, Color.cyan, 0.4f, 3);
    }

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
    public static void SurfaceGizmos(AerodynamicSurfacePhysics surface, GizmoType gizmoType) {
        Rigidbody rigidbody = surface.GetComponentInParent<Rigidbody>();
        AerodynamicSurfaceConfig surfaceConfig = surface.GetAerodynamicSurfaceConfig();

        if (surfaceConfig == null || rigidbody == null) 
            return;

        Gizmos.color = Color.clear;
        Gizmos.matrix = surface.transform.localToWorldMatrix;
        Gizmos.DrawCube(-Vector3.right * 0.25f * surfaceConfig.flapChord, new Vector3(surfaceConfig.flapChord, 0.1f, surfaceConfig.span));

        DrawSurface(surface.transform, surfaceConfig, surface.GetFlapAngle(), surface.IsAtStall);
    }

    private static void DrawSurface(Transform transform, AerodynamicSurfaceConfig config, float flapAngle, bool isAtStall = false) {
        float mainChord = config.flapChord * (1 - config.airfoilChordFraction);
        float flapChord = config.flapChord * config.airfoilChordFraction;

        DrawRectangle(transform.position + transform.right * (0.25f * config.flapChord - 0.5f * mainChord),
                transform.rotation,
                config.span,
                mainChord,
                new Color(27f / 255, 114f / 255, 223f / 255, 0.5f));

        if (config.airfoilChordFraction > 0) {
            DrawRectangle(transform.position
                + transform.right * (0.25f * config.flapChord - mainChord - 0.02f - 0.5f * flapChord * Mathf.Cos(flapAngle))
                - transform.up * 0.5f * Mathf.Sin(flapAngle) * flapChord,
                    transform.rotation * Quaternion.AngleAxis(flapAngle * Mathf.Rad2Deg, Vector3.forward),
                    config.span,
                    flapChord,
                    new Color(203f / 255, 96f / 255, 0f, 0.5f));
        }
    }

    private static void DrawRectangle(Vector3 position, Quaternion rotation, float width, float height, Color color) {
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.DrawSolidRectangleWithOutline(GetRectangleVertices(position, rotation, width, height), color, Color.black);
    }

    private static Vector3[] GetRectangleVertices(Vector3 position, Quaternion rotation, float width, float height) {
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(height, 0, -width) * 0.5f;
        vertices[1] = new Vector3(height, 0, width) * 0.5f;
        vertices[2] = new Vector3(-height, 0, width) * 0.5f;
        vertices[3] = new Vector3(-height, 0, -width) * 0.5f;
        for (int i = 0; i < 4; i++) {
            vertices[i] = rotation * vertices[i] + position;
        }
        return vertices;
    }

    private static void DrawThinArrow(Vector3 position, Vector3 vector, Color color, float headSize, float width) {
        Vector3 vn = vector.normalized;
        Vector3 cross = Vector3.Cross(vn, Camera.current.transform.forward).normalized;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = color;
        Handles.DrawAAPolyLine(width, position, position + vector);
        Handles.DrawAAPolyLine(width, position + vector, position + vector - vn * headSize + cross * headSize * 0.25f);
        Handles.DrawAAPolyLine(width, position + vector, position + vector - vn * headSize - cross * headSize * 0.25f);
    }
}

#endif