using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneData))]
public class SceneDataEditor : Editor {

    #region SerializedProperties

    // General ----------------------------------------------------------------
    SerializedProperty m_sceneConfig;

    // Debug ------------------------------------------------------------------

    // Inputs Jet -------------------------------------------------------------
    // General
    SerializedProperty m_selectedTeam;
    SerializedProperty m_printEveryFrame;

    // Ownship
    SerializedProperty m_printOwnshipDecoys;
    SerializedProperty m_printOwnshipMissiles;
    SerializedProperty m_printOwnshipPitch;
    SerializedProperty m_printOwnshipRoll;
    SerializedProperty m_printOwnshipYaw;
    SerializedProperty m_printOwnshipThrottle;
    SerializedProperty m_printOwnshipPosition;
    SerializedProperty m_printOwnshipVelocity;
    SerializedProperty m_printOwnshipAcceleration;
    SerializedProperty m_printOwnshipRotation;

    // Target
    SerializedProperty m_printTargetDecoys;
    SerializedProperty m_printTargetMissiles;
    SerializedProperty m_printTargetAspectAngle;
    SerializedProperty m_printTargetAntennaTrainAngle;
    SerializedProperty m_printTargetHeadingCrossingAngle;
    SerializedProperty m_printTargetRelativeDistance;
    SerializedProperty m_printTargetRelativeVelocity;
    SerializedProperty m_printTargetRelativeAcceleration;

    // Decoys
    SerializedProperty m_printDecoysInRangeRelativePosition;

    // Missile
    SerializedProperty m_printLaunchedMissileDistanceToTarget;
    SerializedProperty m_printIncomingMissileDistanceToOwnship;
    SerializedProperty m_printIncomingMissileRelativeDirection;

    private bool m_displayDebugGroup = false;

    private bool m_displayInputsGroup = false;
    private bool m_displayOwnshipGroup = false;
    private bool m_displayTargetGroup = false;
    private bool m_displayDecoysGroup = false;
    private bool m_displayMissileGroup = false;

    #endregion

    private void OnEnable() {
        // General ------------------------------------------------------------
        m_sceneConfig = serializedObject.FindProperty("sceneConfig");

        // Inputs Jet ---------------------------------------------------------
        m_selectedTeam = serializedObject.FindProperty("m_selectedTeam");
        m_printEveryFrame = serializedObject.FindProperty("m_printEveryFrame");

        m_printOwnshipDecoys = serializedObject.FindProperty("m_printOwnshipDecoys");
        m_printOwnshipMissiles = serializedObject.FindProperty("m_printOwnshipMissiles");
        m_printOwnshipPitch = serializedObject.FindProperty("m_printOwnshipPitch");
        m_printOwnshipRoll = serializedObject.FindProperty("m_printOwnshipRoll");
        m_printOwnshipYaw = serializedObject.FindProperty("m_printOwnshipYaw");
        m_printOwnshipThrottle = serializedObject.FindProperty("m_printOwnshipThrottle");
        m_printOwnshipPosition = serializedObject.FindProperty("m_printOwnshipPosition");
        m_printOwnshipVelocity = serializedObject.FindProperty("m_printOwnshipVelocity");
        m_printOwnshipAcceleration = serializedObject.FindProperty("m_printOwnshipAcceleration");
        m_printOwnshipRotation = serializedObject.FindProperty("m_printOwnshipRotation");

        m_printTargetDecoys = serializedObject.FindProperty("m_printTargetDecoys");
        m_printTargetMissiles = serializedObject.FindProperty("m_printTargetMissiles");
        m_printTargetAspectAngle = serializedObject.FindProperty("m_printTargetAspectAngle");
        m_printTargetAntennaTrainAngle = serializedObject.FindProperty("m_printTargetAntennaTrainAngle");
        m_printTargetHeadingCrossingAngle = serializedObject.FindProperty("m_printTargetHeadingCrossingAngle");
        m_printTargetRelativeDistance = serializedObject.FindProperty("m_printTargetRelativeDistance");
        m_printTargetRelativeVelocity = serializedObject.FindProperty("m_printTargetRelativeVelocity");
        m_printTargetRelativeAcceleration = serializedObject.FindProperty("m_printTargetRelativeAcceleration");

        m_printDecoysInRangeRelativePosition = serializedObject.FindProperty("m_printDecoysInRangeRelativePosition");

        m_printLaunchedMissileDistanceToTarget = serializedObject.FindProperty("m_printLaunchedMissileDistanceToTarget");
        m_printIncomingMissileDistanceToOwnship = serializedObject.FindProperty("m_printIncomingMissileDistanceToOwnship");
        m_printIncomingMissileRelativeDirection = serializedObject.FindProperty("m_printIncomingMissileRelativeDirection");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_sceneConfig);

        m_displayDebugGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayDebugGroup, "Debug");
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (m_displayDebugGroup) {
            m_displayInputsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayInputsGroup, "Inputs");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayInputsGroup) {
                EditorGUILayout.PropertyField(m_selectedTeam);
                EditorGUILayout.PropertyField(m_printEveryFrame);

                m_displayOwnshipGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayOwnshipGroup, "Ownship");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayOwnshipGroup) {
                    EditorGUILayout.PropertyField(m_printOwnshipDecoys);
                    EditorGUILayout.PropertyField(m_printOwnshipMissiles);
                    EditorGUILayout.PropertyField(m_printOwnshipPitch);
                    EditorGUILayout.PropertyField(m_printOwnshipRoll);
                    EditorGUILayout.PropertyField(m_printOwnshipYaw);
                    EditorGUILayout.PropertyField(m_printOwnshipThrottle);
                    EditorGUILayout.PropertyField(m_printOwnshipPosition);
                    EditorGUILayout.PropertyField(m_printOwnshipVelocity);
                    EditorGUILayout.PropertyField(m_printOwnshipAcceleration);
                    EditorGUILayout.PropertyField(m_printOwnshipRotation);
                }

                m_displayTargetGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayTargetGroup, "Target");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayTargetGroup) {
                    EditorGUILayout.PropertyField(m_printTargetDecoys);
                    EditorGUILayout.PropertyField(m_printTargetMissiles);
                    EditorGUILayout.PropertyField(m_printTargetAspectAngle);
                    EditorGUILayout.PropertyField(m_printTargetAntennaTrainAngle);
                    EditorGUILayout.PropertyField(m_printTargetHeadingCrossingAngle);
                    EditorGUILayout.PropertyField(m_printTargetRelativeDistance);
                    EditorGUILayout.PropertyField(m_printTargetRelativeVelocity);
                    EditorGUILayout.PropertyField(m_printTargetRelativeAcceleration);
                }

                m_displayDecoysGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayDecoysGroup, "Decoys");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayDecoysGroup) {
                    EditorGUILayout.PropertyField(m_printDecoysInRangeRelativePosition);
                }

                m_displayMissileGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayMissileGroup, "Missile");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayMissileGroup) {
                    EditorGUILayout.PropertyField(m_printLaunchedMissileDistanceToTarget);
                    EditorGUILayout.PropertyField(m_printIncomingMissileDistanceToOwnship);
                    EditorGUILayout.PropertyField(m_printIncomingMissileRelativeDirection);
                }

                SceneData sceneData = (SceneData)target;
                if (GUILayout.Button("Print Inputs")) {
                    sceneData.PrintTeamInputs();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

}
