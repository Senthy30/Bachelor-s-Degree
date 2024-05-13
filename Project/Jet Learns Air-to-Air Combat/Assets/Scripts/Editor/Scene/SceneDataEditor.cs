using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneData))]
public class SceneDataEditor : Editor {

    #region GeneralSerializedProperties
    SerializedProperty m_sceneConfig;
    #endregion

    #region DebugSerializedProperties

    #region InputsJetSerializedProperties
    // General
    SerializedProperty m_selectedTeamInputs;
    SerializedProperty m_printInputsEveryFrame;

    // Ownship
    SerializedProperty m_printOwnshipDecoys;
    SerializedProperty m_printOwnshipMissiles;
    SerializedProperty m_printOwnshipPitch;
    SerializedProperty m_printOwnshipRoll;
    SerializedProperty m_printOwnshipYaw;
    SerializedProperty m_printOwnshipFlap;
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

    private bool m_displayInputsGroup = false;
    private bool m_displayOwnshipGroup = false;
    private bool m_displayTargetGroup = false;
    private bool m_displayDecoysGroup = false;
    private bool m_displayMissileGroup = false;
    #endregion


    #region RewardsSerializedProperties

    // General
    SerializedProperty m_selectedTeamRewards;
    SerializedProperty m_printRewardsEveryFrame;
    SerializedProperty m_printTotalReward;

    // Neutral
    SerializedProperty m_printOwnshipInboundReward;

    // Advantage
    SerializedProperty m_printOwnshipAdvantageBehindTargetReward;
    SerializedProperty m_printOwnshipHasTargetInRangeReward;
    SerializedProperty m_printOwnshipChasesTargetReward;

    // Disadvantage
    SerializedProperty m_printTargetHasOwnshipInRangeReward;
    SerializedProperty m_printTargetAdvantageBehindOwnshipReward;
    SerializedProperty m_printTargetChasesOwnshipReward;

    // Groups
    private bool m_displayRewardsGroup = false;
    private bool m_displayNeutralRewardsGroup = false;
    private bool m_displayAdvantageRewardsGroup = false;
    private bool m_displayDisadvantageRewardsGroup = false;

    #endregion


    #region ActionsSerializedProperties

    // General
    SerializedProperty m_selectedTeamActions;
    SerializedProperty m_printActionsEveryFrame;

    SerializedProperty m_printRollAction;
    SerializedProperty m_printPitchAction;
    SerializedProperty m_printYawAction;
    SerializedProperty m_printThrustAction;
    SerializedProperty m_printFlapsAction;
    SerializedProperty m_printLaunchMissileAction;
    SerializedProperty m_printDropDecoyAction;

    // Groups

    private bool m_displayActionsGroup = false;

    #endregion


    // Groups
    private bool m_displayDebugGroup = false;

    #endregion

    private void OnEnable() {
        // General ------------------------------------------------------------
        m_sceneConfig = serializedObject.FindProperty("sceneConfig");

        // Debug --------------------------------------------------------------
        // Inputs Jet ---------------------------------------------------------
        m_selectedTeamInputs = serializedObject.FindProperty("m_selectedTeamInputs");
        m_printInputsEveryFrame = serializedObject.FindProperty("m_printInputsEveryFrame");

        // Ownship
        m_printOwnshipDecoys = serializedObject.FindProperty("m_printOwnshipDecoys");
        m_printOwnshipMissiles = serializedObject.FindProperty("m_printOwnshipMissiles");
        m_printOwnshipPitch = serializedObject.FindProperty("m_printOwnshipPitch");
        m_printOwnshipRoll = serializedObject.FindProperty("m_printOwnshipRoll");
        m_printOwnshipYaw = serializedObject.FindProperty("m_printOwnshipYaw");
        m_printOwnshipFlap = serializedObject.FindProperty("m_printOwnshipFlap");
        m_printOwnshipThrottle = serializedObject.FindProperty("m_printOwnshipThrottle");
        m_printOwnshipPosition = serializedObject.FindProperty("m_printOwnshipPosition");
        m_printOwnshipVelocity = serializedObject.FindProperty("m_printOwnshipVelocity");
        m_printOwnshipAcceleration = serializedObject.FindProperty("m_printOwnshipAcceleration");
        m_printOwnshipRotation = serializedObject.FindProperty("m_printOwnshipRotation");

        // Target
        m_printTargetDecoys = serializedObject.FindProperty("m_printTargetDecoys");
        m_printTargetMissiles = serializedObject.FindProperty("m_printTargetMissiles");
        m_printTargetAspectAngle = serializedObject.FindProperty("m_printTargetAspectAngle");
        m_printTargetAntennaTrainAngle = serializedObject.FindProperty("m_printTargetAntennaTrainAngle");
        m_printTargetHeadingCrossingAngle = serializedObject.FindProperty("m_printTargetHeadingCrossingAngle");
        m_printTargetRelativeDistance = serializedObject.FindProperty("m_printTargetRelativeDistance");
        m_printTargetRelativeVelocity = serializedObject.FindProperty("m_printTargetRelativeVelocity");
        m_printTargetRelativeAcceleration = serializedObject.FindProperty("m_printTargetRelativeAcceleration");

        // Decoys
        m_printDecoysInRangeRelativePosition = serializedObject.FindProperty("m_printDecoysInRangeRelativePosition");

        // Missile
        m_printLaunchedMissileDistanceToTarget = serializedObject.FindProperty("m_printLaunchedMissileDistanceToTarget");
        m_printIncomingMissileDistanceToOwnship = serializedObject.FindProperty("m_printIncomingMissileDistanceToOwnship");
        m_printIncomingMissileRelativeDirection = serializedObject.FindProperty("m_printIncomingMissileRelativeDirection");
    
        // Rewards Jet --------------------------------------------------------
        m_selectedTeamRewards = serializedObject.FindProperty("m_selectedTeamRewards");
        m_printRewardsEveryFrame = serializedObject.FindProperty("m_printRewardsEveryFrame");
        m_printTotalReward = serializedObject.FindProperty("m_printTotalReward");

        // Neutral
        m_printOwnshipInboundReward = serializedObject.FindProperty("m_printOwnshipInboundReward");
        
        // Advantage
        m_printOwnshipAdvantageBehindTargetReward = serializedObject.FindProperty("m_printOwnshipAdvantageBehindTargetReward");
        m_printOwnshipHasTargetInRangeReward = serializedObject.FindProperty("m_printOwnshipHasTargetInRangeReward");
        m_printOwnshipChasesTargetReward = serializedObject.FindProperty("m_printOwnshipChasesTargetReward");

        // Disadvantage
        m_printTargetAdvantageBehindOwnshipReward = serializedObject.FindProperty("m_printTargetAdvantageBehindOwnshipReward");
        m_printTargetHasOwnshipInRangeReward = serializedObject.FindProperty("m_printTargetHasOwnshipInRangeReward");
        m_printTargetChasesOwnshipReward = serializedObject.FindProperty("m_printTargetChasesOwnshipReward");

        // Actions Jet --------------------------------------------------------
        m_selectedTeamActions = serializedObject.FindProperty("m_selectedTeamActions");
        m_printActionsEveryFrame = serializedObject.FindProperty("m_printActionsEveryFrame");

        m_printRollAction = serializedObject.FindProperty("m_printRollAction");
        m_printPitchAction = serializedObject.FindProperty("m_printPitchAction");
        m_printYawAction = serializedObject.FindProperty("m_printYawAction");
        m_printThrustAction = serializedObject.FindProperty("m_printThrustAction");
        m_printFlapsAction = serializedObject.FindProperty("m_printFlapsAction");
        m_printLaunchMissileAction = serializedObject.FindProperty("m_printLaunchMissileAction");
        m_printDropDecoyAction = serializedObject.FindProperty("m_printDropDecoyAction");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_sceneConfig);

        // Debug --------------------------------------------------------------
        m_displayDebugGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayDebugGroup, "Debug");
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (m_displayDebugGroup) {
            // Inputs Jet -----------------------------------------------------
            m_displayInputsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayInputsGroup, "Inputs");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayInputsGroup) {
                EditorGUILayout.PropertyField(m_selectedTeamInputs);
                EditorGUILayout.PropertyField(m_printInputsEveryFrame);

                // Ownship
                m_displayOwnshipGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayOwnshipGroup, "Ownship");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayOwnshipGroup) {
                    EditorGUILayout.PropertyField(m_printOwnshipDecoys);
                    EditorGUILayout.PropertyField(m_printOwnshipMissiles);
                    EditorGUILayout.PropertyField(m_printOwnshipPitch);
                    EditorGUILayout.PropertyField(m_printOwnshipRoll);
                    EditorGUILayout.PropertyField(m_printOwnshipYaw);
                    EditorGUILayout.PropertyField(m_printOwnshipFlap);
                    EditorGUILayout.PropertyField(m_printOwnshipThrottle);
                    EditorGUILayout.PropertyField(m_printOwnshipPosition);
                    EditorGUILayout.PropertyField(m_printOwnshipVelocity);
                    EditorGUILayout.PropertyField(m_printOwnshipAcceleration);
                    EditorGUILayout.PropertyField(m_printOwnshipRotation);
                }

                // Target
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

                // Decoys
                m_displayDecoysGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayDecoysGroup, "Decoys");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayDecoysGroup) {
                    EditorGUILayout.PropertyField(m_printDecoysInRangeRelativePosition);
                }

                // Missile
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

            // Rewards Jet ----------------------------------------------------
            m_displayRewardsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayRewardsGroup, "Rewards");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayRewardsGroup) {
                EditorGUILayout.PropertyField(m_selectedTeamRewards);
                EditorGUILayout.PropertyField(m_printRewardsEveryFrame);
                EditorGUILayout.PropertyField(m_printTotalReward);

                // Neutral
                m_displayNeutralRewardsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayNeutralRewardsGroup, "Neutral");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayNeutralRewardsGroup) {
                    EditorGUILayout.PropertyField(m_printOwnshipInboundReward);
                }

                // Advantage
                m_displayAdvantageRewardsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayAdvantageRewardsGroup, "Advantage");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayAdvantageRewardsGroup) {
                    EditorGUILayout.PropertyField(m_printOwnshipAdvantageBehindTargetReward);
                    EditorGUILayout.PropertyField(m_printOwnshipHasTargetInRangeReward);
                    EditorGUILayout.PropertyField(m_printOwnshipChasesTargetReward);
                }

                // Disadvantage
                m_displayDisadvantageRewardsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayDisadvantageRewardsGroup, "Disadvantage");
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (m_displayDisadvantageRewardsGroup) {
                    EditorGUILayout.PropertyField(m_printTargetAdvantageBehindOwnshipReward);
                    EditorGUILayout.PropertyField(m_printTargetHasOwnshipInRangeReward);
                    EditorGUILayout.PropertyField(m_printTargetChasesOwnshipReward);
                }
            }

            // Actions Jet ----------------------------------------------------
            m_displayActionsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayActionsGroup, "Actions");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayActionsGroup) {
                EditorGUILayout.PropertyField(m_selectedTeamActions);
                EditorGUILayout.PropertyField(m_printActionsEveryFrame);

                EditorGUILayout.PropertyField(m_printRollAction);
                EditorGUILayout.PropertyField(m_printPitchAction);
                EditorGUILayout.PropertyField(m_printYawAction);
                EditorGUILayout.PropertyField(m_printThrustAction);
                EditorGUILayout.PropertyField(m_printFlapsAction);
                EditorGUILayout.PropertyField(m_printLaunchMissileAction);
                EditorGUILayout.PropertyField(m_printDropDecoyAction);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

}
