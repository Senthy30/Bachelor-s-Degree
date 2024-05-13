#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(JetAgent))]
public class JetAgentEditor : Editor {

    #region Jet Inputs

    private JetInputs m_jetInputs;

    private bool m_displayJetInputs = true;
    private bool m_displayJetInputsOwnship = false;
    private bool m_displayJetInputsTarget = false;
    private bool m_displayJetInputsDecoys = false;
    private bool m_displayJetInputsMissile = false;

    private void OnInspectorGUIInputsGroup() {
        m_displayJetInputs = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetInputs, "Observations");
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (m_displayJetInputs) {
            m_displayJetInputsOwnship = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetInputsOwnship, "Ownship");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayJetInputsOwnship) {
                EditorGUILayout.LabelField("Booleans", EditorStyles.boldLabel);
                EditorGUILayout.Toggle("Flaps Active", m_jetInputs.areFlapsActive);
                EditorGUILayout.Toggle("Landing Gear Active", m_jetInputs.areLandingGearActive);

                EditorGUILayout.LabelField("Floats", EditorStyles.boldLabel);
                EditorGUILayout.FloatField("Decoys", m_jetInputs.decoys);
                EditorGUILayout.FloatField("Missiles", m_jetInputs.missiles);

                EditorGUILayout.FloatField("Pitch", m_jetInputs.pitch);
                EditorGUILayout.FloatField("Roll", m_jetInputs.roll);
                EditorGUILayout.FloatField("Yaw", m_jetInputs.yaw);
                EditorGUILayout.FloatField("Flap", m_jetInputs.flap);
                EditorGUILayout.FloatField("Throttle", m_jetInputs.throttle);

                EditorGUILayout.LabelField("Vectors", EditorStyles.boldLabel);
                EditorGUILayout.Vector3Field("Position", m_jetInputs.position);
                EditorGUILayout.Vector3Field("Velocity", m_jetInputs.velocity);
                EditorGUILayout.Vector3Field("Angular Velocity", m_jetInputs.angularVelocity);
                EditorGUILayout.Vector3Field("Acceleration", m_jetInputs.acceleration);
                EditorGUILayout.Vector3Field("Angular Acceleration", m_jetInputs.angularAcceleration);
                EditorGUILayout.Vector4Field("Rotation", new Vector4(m_jetInputs.rotation.x, m_jetInputs.rotation.y, m_jetInputs.rotation.z, m_jetInputs.rotation.w));
            }

            m_displayJetInputsTarget = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetInputsTarget, "Target");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayJetInputsTarget) { 
                EditorGUILayout.LabelField("Floats", EditorStyles.boldLabel);
                EditorGUILayout.FloatField("Decoys Target", m_jetInputs.decoysTarget);
                EditorGUILayout.FloatField("Missiles Target", m_jetInputs.missilesTarget);

                EditorGUILayout.FloatField("Aspect Angle", m_jetInputs.aspectAngle);
                EditorGUILayout.FloatField("Antenna Train Angle", m_jetInputs.antennaTrainAngle);
                EditorGUILayout.FloatField("Heading Crossing Angle", m_jetInputs.headingCrossingAngle);
                EditorGUILayout.FloatField("Relative Altitude", m_jetInputs.relativeAltitude);
                EditorGUILayout.FloatField("Relative Distance", m_jetInputs.relativeDistance);

                EditorGUILayout.LabelField("Vectors", EditorStyles.boldLabel);
                EditorGUILayout.Vector3Field("Relative Velocity", m_jetInputs.relativeVelocity);
                EditorGUILayout.Vector3Field("Relative Angular Velocity", m_jetInputs.relativeAngularVelocity);
                EditorGUILayout.Vector3Field("Relative Acceleration", m_jetInputs.relativeAcceleration);
                EditorGUILayout.Vector3Field("Relative Angular Acceleration", m_jetInputs.relativeAngularAcceleration);
                EditorGUILayout.Vector3Field("Relative Direction", m_jetInputs.relativeDirection);
                EditorGUILayout.Vector4Field("Relative Rotation", new Vector4(m_jetInputs.relativeRotation.x, m_jetInputs.relativeRotation.y, m_jetInputs.relativeRotation.z, m_jetInputs.relativeRotation.w));
            }

            m_displayJetInputsDecoys = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetInputsDecoys, "Decoys");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayJetInputsDecoys) {
                EditorGUILayout.LabelField("Booleans", EditorStyles.boldLabel);
                EditorGUILayout.Toggle("Is Decoy In Range", m_jetInputs.isDecoyInRange);

                EditorGUILayout.LabelField("Floats", EditorStyles.boldLabel);
                EditorGUILayout.FloatField("In Range Decoy Distance", m_jetInputs.inRangeDecoyDistance);
                EditorGUILayout.FloatField("In Range Decoy Angle", m_jetInputs.inRangeDecoyAngle);

                EditorGUILayout.LabelField("Vectors", EditorStyles.boldLabel);
                EditorGUILayout.Vector3Field("In Range Decoy Relative Direction", m_jetInputs.inRangeDecoyRelativeDirection);
            }

            m_displayJetInputsMissile = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetInputsMissile, "Missile");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayJetInputsMissile) {
                EditorGUILayout.LabelField("Booleans", EditorStyles.boldLabel);
                EditorGUILayout.Toggle("Has Launched Missile In Range Target", m_jetInputs.hasLaunchedMissileInRangeTarget);
                EditorGUILayout.Toggle("Is Missile Incoming To Ownship", m_jetInputs.hasIncomingMissileInRangeOwnship);

                EditorGUILayout.LabelField("Floats", EditorStyles.boldLabel);
                EditorGUILayout.FloatField("Launched Missile Distance To Target", m_jetInputs.launchedMissileDistanceToTarget);
                EditorGUILayout.FloatField("Incoming Missile Distance To Ownship", m_jetInputs.incomigMissileDistanceToOwnship);
                EditorGUILayout.FloatField("Incoming Missile Angle To Ownship", m_jetInputs.incomingMissileAngleToOwnship);

                EditorGUILayout.LabelField("Vectors", EditorStyles.boldLabel);
                EditorGUILayout.Vector3Field("Incoming Missile Relative Direction", m_jetInputs.incomingMissileRelativeDirection);
                EditorGUILayout.Vector3Field("Incoming Missile Relative Velocity", m_jetInputs.incomingMissileRelativeVelocity);
                EditorGUILayout.Vector3Field("Incoming Missile Relative Acceleration", m_jetInputs.incomingMissileRelativeAcceleration);
            }
        }
    }

    #endregion


    #region Jet Rewards

    private JetRewards m_jetRewards;

    private bool m_displayJetRewards = true;
    private bool m_displayJetRewardsOwnship = false;
    private bool m_displayJetRewardsTarget = false;

    private void OnInspectorGUIRewardsGroup() {
        m_displayJetRewards = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetRewards, "Rewards");
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (m_displayJetRewards) {
            EditorGUILayout.FloatField("Total", m_jetRewards.total);

            m_displayJetRewardsOwnship = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetRewardsOwnship, "Ownship");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayJetRewardsOwnship) {
                EditorGUILayout.FloatField("Inbound", m_jetRewards.inbound);
                EditorGUILayout.FloatField("Advantage Behind Target", m_jetRewards.ownshipAdvantageBehindTarget);
                EditorGUILayout.FloatField("Has Target In Range", m_jetRewards.ownshipHasTargetInRange);
                EditorGUILayout.FloatField("Chases Target", m_jetRewards.ownshipChasesTarget);
                EditorGUILayout.FloatField("Launches Missile", m_jetRewards.ownshipLaunchesMissile);
                EditorGUILayout.FloatField("Drops Decoy", m_jetRewards.ownshipDropsDecoy);
                EditorGUILayout.FloatField("Shirks Incoming Missile", m_jetRewards.ownshipShirksIncomingMissile);
            }

            m_displayJetRewardsTarget = EditorGUILayout.BeginFoldoutHeaderGroup(m_displayJetRewardsTarget, "Target");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (m_displayJetRewardsTarget) {
                EditorGUILayout.FloatField("Advantage Behind Ownship", m_jetRewards.targetAdvantageBehindOwnship);
                EditorGUILayout.FloatField("Has Ownship In Range", m_jetRewards.targetHasOwnshipInRange);
                EditorGUILayout.FloatField("Chases Ownship", m_jetRewards.targetChasesOwnship);
                EditorGUILayout.FloatField("Launches Missile", m_jetRewards.targetLaunchesMissile);
                EditorGUILayout.FloatField("Shirks Missile", m_jetRewards.targetShirksMissile);
            }
        }
    }

    #endregion

    public override void OnInspectorGUI() {
        JetAgent aircraftAgent = (JetAgent)target;
        JetData aircraftData = aircraftAgent.GetJetData();

        DrawDefaultInspector();

        if (aircraftData != null) {
            m_jetInputs = aircraftData.GetJetInputs();
            m_jetRewards = aircraftData.GetJetRewards();

            OnInspectorGUIInputsGroup();
            OnInspectorGUIRewardsGroup();
        }
    }

}

#endif

