using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class JetAgent : Agent {

    private SceneData m_sceneData;

    private bool m_exploded = false;

    private bool m_controlledByPlayer = false;
    private bool m_previousCollectObservationsWasCalled = false;
    private Team m_team;
    private JetData m_jetData;
    private JetCollision m_jetCollision;
    private AircraftController m_aircraftController;

    public override void CollectObservations(VectorSensor sensor) {
        JetInputs jetInputs = new JetInputs();
        if (m_sceneData != null && m_jetData != null) {
            jetInputs = SceneInputs.RequestJetInputs(m_sceneData, m_team);
        }
        else {
            Debug.LogError("SceneData or JetData is null");
        }

        // Ownship
        // bool
        sensor.AddObservation(jetInputs.areFlapsActive);
        sensor.AddObservation(jetInputs.areLandingGearActive);

        // int

        sensor.AddObservation(jetInputs.decoys);
        sensor.AddObservation(jetInputs.missiles);

        // float
        sensor.AddObservation(jetInputs.pitch);
        sensor.AddObservation(jetInputs.roll);
        sensor.AddObservation(jetInputs.yaw);
        sensor.AddObservation(jetInputs.flap);
        sensor.AddObservation(jetInputs.throttle);

        // Vector3
        sensor.AddObservation(jetInputs.position);
        sensor.AddObservation(jetInputs.velocity);
        sensor.AddObservation(jetInputs.angularVelocity);
        sensor.AddObservation(jetInputs.acceleration);
        sensor.AddObservation(jetInputs.angularAcceleration);
        sensor.AddObservation(jetInputs.rotation);

        // Target
        // int
        sensor.AddObservation(jetInputs.decoysTarget);
        sensor.AddObservation(jetInputs.missilesTarget);

        // float
        sensor.AddObservation(jetInputs.aspectAngle);
        sensor.AddObservation(jetInputs.antennaTrainAngle);
        sensor.AddObservation(jetInputs.headingCrossingAngle);
        sensor.AddObservation(jetInputs.relativeAltitude);
        sensor.AddObservation(jetInputs.relativeDistance);

        // Vector3
        sensor.AddObservation(jetInputs.relativeVelocity);
        sensor.AddObservation(jetInputs.relativeAngularVelocity);
        sensor.AddObservation(jetInputs.relativeAcceleration);
        sensor.AddObservation(jetInputs.relativeAngularAcceleration);
        sensor.AddObservation(jetInputs.relativeDirection);
        sensor.AddObservation(jetInputs.relativeRotation);

        // Decoys
        // bool
        sensor.AddObservation(jetInputs.isDecoyInRange);

        // float
        sensor.AddObservation(jetInputs.inRangeDecoyDistance);
        sensor.AddObservation(jetInputs.inRangeDecoyAngle);

        // Vector3
        sensor.AddObservation(jetInputs.inRangeDecoyRelativeDirection);

        // Missile
        // bool
        sensor.AddObservation(jetInputs.hasLaunchedMissileInRangeTarget);
        sensor.AddObservation(jetInputs.hasIncomingMissileInRangeOwnship);

        // float
        sensor.AddObservation(jetInputs.launchedMissileDistanceToTarget);
        sensor.AddObservation(jetInputs.incomigMissileDistanceToOwnship);
        sensor.AddObservation(jetInputs.incomingMissileAngleToOwnship);

        // Vector3
        sensor.AddObservation(jetInputs.incomingMissileRelativeDirection);
        sensor.AddObservation(jetInputs.incomingMissileRelativeVelocity);
        sensor.AddObservation(jetInputs.incomingMissileRelativeAcceleration);

        m_previousCollectObservationsWasCalled = true;
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        if (!m_controlledByPlayer)
            return;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        JetAction jetAction = m_jetData.GetJetAction();
        JetAction.CalculateHeuristicActions(jetAction);

        discreteActions[0] = (int)jetAction.GetRollAction();
        discreteActions[1] = (int)jetAction.GetPitchAction();
        discreteActions[2] = (int)jetAction.GetYawAction();
        discreteActions[3] = (int)jetAction.GetThrustAction();
        discreteActions[4] = (int)jetAction.GetFlapsAction();
        discreteActions[5] = (int)jetAction.GetLaunchMissileAction();
        discreteActions[6] = (int)jetAction.GetDropDecoyAction();
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if (m_exploded) {
            AddReward(NormalizeRewardValue(SceneRewards.GetRewardsConfig().destroyHimself));
            return;
        }

        JetAction jetAction = m_jetData.GetJetAction();
        EnemiesData enemiesData = m_sceneData.GetSceneComponents().GetEnemiesData(m_team, GetTargetTeam(m_team));

        jetAction.SetRollAction(actions.DiscreteActions[0]);
        jetAction.SetPitchAction(actions.DiscreteActions[1]);
        jetAction.SetYawAction(actions.DiscreteActions[2]);
        jetAction.SetThrustAction(actions.DiscreteActions[3]);
        jetAction.SetFlapsAction(actions.DiscreteActions[4]);
        jetAction.SetLaunchMissileAction(actions.DiscreteActions[5]);
        jetAction.SetDropDecoyAction(actions.DiscreteActions[6]);

        if (!m_previousCollectObservationsWasCalled) {
            jetAction.SetLaunchMissileAction(JetAction.LaunchMissileAction.None);
            jetAction.SetDropDecoyAction(JetAction.DropDecoyAction.None);
        }
        m_previousCollectObservationsWasCalled = false;

        JetController.PerformJetActions(m_aircraftController, jetAction);
        enemiesData.UpdateDataAfterAction();

        JetRewards jetRewards = SceneRewards.RequestJetReward(m_sceneData, enemiesData);
        m_jetData.SetJetRewards(jetRewards);
        AddReward(NormalizeRewardValue(jetRewards.total));

        enemiesData.UpdateDataForNextAction();
    }

    public void ConfigureOnExplosion() {
        m_exploded = true;
        
        AddReward(NormalizeRewardValue(SceneRewards.GetRewardsConfig().destroyHimself));
        EndEpisode();
    }

    public void ConfigureOnStart(AircraftController aircraftController) {
        m_aircraftController = aircraftController;

        m_sceneData = m_aircraftController.GetSceneData();

        m_jetData = m_aircraftController.GetJetData();
        m_jetCollision = m_jetData.GetJetCollision();
        m_team = m_jetData.GetTeam();
    }

    public void ConfigureOnEpisodeEnd() {
        if (!m_exploded && gameObject.activeSelf) {
            EpisodeInterrupted();
        }
            
        m_exploded = false;
    }

    #region Getters

    public JetData GetJetData() {
        return m_jetData;
    }

    #endregion

    #region Setters

    public void SetControlledByPlayer(bool controlledByPlayer) {
        m_controlledByPlayer = controlledByPlayer;
    }

    public void SetJetData(JetData jetData) {
        m_jetData = jetData;
    }

    #endregion

    #region Static Methods

    public static Team GetTargetTeam(Team ownshipTeam) {
        if (ownshipTeam == Team.BLUE) {
            return Team.RED;
        } else {
            return Team.BLUE;
        }
    }

    public static JetData GetJetDataTargetTeam(SceneData sceneData, Team ownshipTeam) {
        if (ownshipTeam == Team.BLUE) {
            return sceneData.GetSceneComponents().GetJetData(Team.RED);
        } else {
            return sceneData.GetSceneComponents().GetJetData(Team.BLUE);
        }
    }

    public static float NormalizeRewardValue(float reward) {
        return reward / 10000f;
    }

    #endregion
}
