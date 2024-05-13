using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Scenario {
    RANDOM = -1,
    TAKE_OFF,
    BLUE_CHASING,
    RED_CHASING,
    BLUE_ATTACKING,
    RED_ATTACKING,
    FLIGHTING,
    MAX
}

public class SceneData : MonoBehaviour {

    private const float maxSimulationTime = 300.0f;
    private static bool randomScenario;
    private static int nextIdScene = 0;
    private static float rebuildSceneAfterSeconds = 5.0f;

    private bool rebuildingScene;
    private int idScene;
    private int numJetsExploded = 0;
    private float timePassed = 0.0f;
    private Scenario scenario;

    [SerializeField] private SceneConfig sceneConfig;
    private SceneBuilder sceneBuilder = null;
    private SceneComponents sceneComponents = null;

    void Awake() {
        BuildScene();
    }

    void Update() {
        if (idScene != 0)
            return;

        if (m_printInputsEveryFrame)
            PrintTeamInputs();

        if (m_printRewardsEveryFrame)
            PrintTeamRewards();

        if (m_printActionsEveryFrame)
            PrintTeamActions();

        //Debug.Log("Cumulative reward: " + sceneComponents.GetJetData(Team.BLUE).GetObject().GetComponent<JetAgent>().GetCumulativeReward());
    }

    private void FixedUpdate() {
        timePassed += Time.fixedDeltaTime;
        if (timePassed >= maxSimulationTime) {
            rebuildingScene = false;
            numJetsExploded = 0;
            timePassed = 0.0f;
            ResetEnemiesDataValues();
            sceneBuilder.RebuildScene();
        }

        m_debugMode.SetTimeRemaining(maxSimulationTime - timePassed);
    }

    public void BuildScene() {
        if (m_debugMode == null)
            m_debugMode = FindObjectOfType<DebugMode>();
        idScene = nextIdScene++;
        numJetsExploded = 0;
        rebuildingScene = false;

        SetResolutionSceneConfig();
        SceneComponents.SetSceneConfig(sceneConfig);
        if (!randomScenario) {
            scenario = FindObjectOfType<TheaterData>().GetScenario();
        } else {
            scenario = Scenario.RANDOM;
        }

        sceneBuilder = new SceneBuilder(idScene, gameObject, sceneConfig);
    }

    public void DropDecoy(Team team) {
        JetData jetData = sceneComponents.GetJetData(team);

        if (jetData.GetNumDecoys() > 0) {
            jetData.DecrementNumDecoys();

            DecoyData decoyData = sceneBuilder.BuildDecoy(jetData.GetDecoySpawnPointTransform().position);
            DecoyPhysics decoyPhysics = decoyData.GetObject().GetComponent<DecoyPhysics>();

            decoyPhysics.AddJetCollision(jetData.GetColliderParentObject());
            decoyPhysics.SetVelocity(jetData.GetObject().GetComponent<Rigidbody>().velocity, -jetData.GetObject().transform.up - jetData.GetObject().transform.forward);
            decoyPhysics.SetCollisionsIgnoreAdded();

            sceneComponents.AddDecoyData(decoyData);
            sceneComponents.TriggerMissilesFindTarget();
        }
    }

    public void DestroyDecoy(DecoyData decoyData) {
        sceneComponents.RemoveDecoyData(decoyData);
        sceneComponents.RemoveHeatEmission(decoyData.GetHeatEmission());
    }

    public void JetExploded(Team team) {
        ++numJetsExploded;
        if (!rebuildingScene && numJetsExploded >= TheaterData.GetNumTeams() - 1) {
            rebuildingScene = true;
            CallRebuildSceneImmediately();
            //StartCoroutine(CallRebuildSceneAfterSeconds(rebuildSceneAfterSeconds));
        }
    }

    void CallRebuildSceneImmediately() {
        if (rebuildingScene) {
            rebuildingScene = false;
            numJetsExploded = 0;
            timePassed = 0.0f;
            ResetEnemiesDataValues();
            sceneBuilder.RebuildScene();
        }
    }

    IEnumerator CallRebuildSceneAfterSeconds(float delay) {
        yield return new WaitForSeconds(delay);

        if (rebuildingScene) {
            rebuildingScene = false;
            numJetsExploded = 0;
            timePassed = 0.0f;
            ResetEnemiesDataValues();
            sceneBuilder.RebuildScene();
        }
    }

    private void ResetEnemiesDataValues() {
        foreach (Team ownshipTeam in System.Enum.GetValues(typeof(Team))) {
            foreach (Team targetTeam in System.Enum.GetValues(typeof(Team))) {
                if (ownshipTeam == targetTeam) {
                    continue;
                }

                EnemiesData enemiesData = sceneComponents.GetEnemiesData(ownshipTeam, targetTeam);

                enemiesData.distance = float.MaxValue;
                enemiesData.angleOwnshipBehindTarget = float.MaxValue;
                enemiesData.angleOwnshipToTarget = float.MaxValue;
                enemiesData.angleTargetBehindOwnship = float.MaxValue;
                enemiesData.angleTargetToOwnship = float.MaxValue;
                enemiesData.ownshipIncomingMissile = false;
                enemiesData.targetIncomingMissile = false;
            }
        }
    }

    // Setters ------------------------------------------------

    public void SetScenario(Scenario scenario) {
        this.scenario = scenario;
    }

    public void SetSceneComponents(SceneComponents sceneComponents) {
        this.sceneComponents = sceneComponents;
    }

    public void SetResolutionSceneConfig() {
        if (sceneConfig.resolution != TheaterData.GetResolution())
            sceneConfig.resolution = TheaterData.GetResolution();
    }

    public static void SetRandomScenario(bool random) {
        randomScenario = random;
    }

    public static void SetNextIdScene(int id) {
        nextIdScene = id;
    }

    public static void SetRebuildSceneAfterSeconds(float seconds) {
        rebuildSceneAfterSeconds = seconds;
    }

    // Getters ------------------------------------------------

    public int GetIdScene() {
        return idScene;
    }

    public Scenario GetScenario() {
        return scenario;
    }

    public SceneComponents GetSceneComponents() {
        return sceneComponents;
    }

    public SceneConfig GetSceneConfig() {
        return sceneConfig;
    }

    public float GetMaxDecoyViewAngle() {
        return sceneConfig.maxDecoyViewAngle;
    }

    // Debug --------------------------------------------------
    private static DebugMode m_debugMode = null;

    // Inputs -------------------------------------------------
    [SerializeField] private Team m_selectedTeamInputs;
    [SerializeField] private bool m_printInputsEveryFrame = false;

    [SerializeField] private bool m_printOwnshipDecoys = false;
    [SerializeField] private bool m_printOwnshipMissiles = false;
    [SerializeField] private bool m_printOwnshipPitch = false;
    [SerializeField] private bool m_printOwnshipRoll = false;
    [SerializeField] private bool m_printOwnshipYaw = false;
    [SerializeField] private bool m_printOwnshipFlap = false;
    [SerializeField] private bool m_printOwnshipThrottle = false;
    [SerializeField] private bool m_printOwnshipPosition = false;
    [SerializeField] private bool m_printOwnshipVelocity = false;
    [SerializeField] private bool m_printOwnshipAcceleration = false;
    [SerializeField] private bool m_printOwnshipRotation = false;

    [SerializeField] private bool m_printTargetDecoys = false;
    [SerializeField] private bool m_printTargetMissiles = false;
    [SerializeField] private bool m_printTargetAspectAngle = false;
    [SerializeField] private bool m_printTargetAntennaTrainAngle = false;
    [SerializeField] private bool m_printTargetHeadingCrossingAngle = false;
    [SerializeField] private bool m_printTargetRelativeDistance = false;
    [SerializeField] private bool m_printTargetRelativeVelocity = false;
    [SerializeField] private bool m_printTargetRelativeAcceleration = false;

    [SerializeField] private bool m_printDecoysInRangeRelativePosition = false;

    [SerializeField] private bool m_printLaunchedMissileDistanceToTarget = false;
    [SerializeField] private bool m_printIncomingMissileDistanceToOwnship = false;
    [SerializeField] private bool m_printIncomingMissileRelativeDirection = false;

    public void PrintTeamInputs() {
        JetInputs jetInputs = SceneInputs.RequestJetInputs(this, m_selectedTeamInputs);

        if (m_printOwnshipDecoys)
            Debug.Log("[OWNSHIP] Decoys: " + jetInputs.decoys);
        if (m_printOwnshipMissiles)
            Debug.Log("[OWNSHIP] Missiles: " + jetInputs.missiles);
        if (m_printOwnshipPitch)
            Debug.Log("[OWNSHIP] Pitch: " + jetInputs.pitch);
        if (m_printOwnshipRoll)
            Debug.Log("[OWNSHIP] Roll: " + jetInputs.roll);
        if (m_printOwnshipYaw)
            Debug.Log("[OWNSHIP] Yaw: " + jetInputs.yaw);
        if (m_printOwnshipFlap)
            Debug.Log("[OWNSHIP] Flap: " + jetInputs.flap);
        if (m_printOwnshipThrottle)
            Debug.Log("[OWNSHIP] Throttle: " + jetInputs.throttle);
        if (m_printOwnshipPosition)
            Debug.Log("[OWNSHIP] Position: " + jetInputs.position);
        if (m_printOwnshipVelocity)
            Debug.Log("[OWNSHIP] Velocity: " + jetInputs.velocity);
        if (m_printOwnshipAcceleration)
            Debug.Log("[OWNSHIP] Acceleration: " + jetInputs.acceleration);
        if (m_printOwnshipRotation)
            Debug.Log("[OWNSHIP] Rotation: " + jetInputs.rotation);

        if (m_printTargetDecoys)
            Debug.Log("[TARGET] Decoys: " + jetInputs.decoysTarget);
        if (m_printTargetMissiles)
            Debug.Log("[TARGET] Missiles: " + jetInputs.missilesTarget);
        if (m_printTargetAspectAngle)
            Debug.Log("[TARGET] Aspect Angle: " + jetInputs.aspectAngle);
        if (m_printTargetAntennaTrainAngle)
            Debug.Log("[TARGET] Antenna Train Angle: " + jetInputs.antennaTrainAngle);
        if (m_printTargetHeadingCrossingAngle)
            Debug.Log("[TARGET] Heading Crossing Angle: " + jetInputs.headingCrossingAngle);
        if (m_printTargetRelativeDistance)
            Debug.Log("[TARGET] Relative Distance: " + jetInputs.relativeDistance);
        if (m_printTargetRelativeVelocity)
            Debug.Log("[TARGET] Relative Velocity: " + jetInputs.relativeVelocity);
        if (m_printTargetRelativeAcceleration)
            Debug.Log("[TARGET] Relative Acceleration: " + jetInputs.relativeAcceleration);

        if (m_printDecoysInRangeRelativePosition)
            Debug.Log("[DECOYS] In Range Relative Position: " + jetInputs.inRangeDecoyRelativeDirection);

        if (m_printLaunchedMissileDistanceToTarget)
            Debug.Log("[MISSILE] Launched Missile Distance To Target: " + jetInputs.launchedMissileDistanceToTarget);
        if (m_printIncomingMissileDistanceToOwnship)
            Debug.Log("[MISSILE] Incoming Missile Distance To Ownship: " + jetInputs.incomigMissileDistanceToOwnship);
        if (m_printIncomingMissileRelativeDirection)
            Debug.Log("[MISSILE] Incoming Missile Relative Position: " + jetInputs.incomingMissileRelativeDirection);
    }

    // Rewards ----------------------------------------------------------------
    [SerializeField] private Team m_selectedTeamRewards;
    [SerializeField] private bool m_printRewardsEveryFrame = false;
    [SerializeField] private bool m_printTotalReward = false;

    // Neutral Values ---------------------------------------------------------
    [SerializeField] private bool m_printOwnshipInboundReward = false;

    // Advantage Values -------------------------------------------------------
    [SerializeField] private bool m_printOwnshipAdvantageBehindTargetReward = false;
    [SerializeField] private bool m_printOwnshipHasTargetInRangeReward = false;
    [SerializeField] private bool m_printOwnshipChasesTargetReward = false;

    // Disadvantage Values ----------------------------------------------------
    [SerializeField] private bool m_printTargetAdvantageBehindOwnshipReward = false;
    [SerializeField] private bool m_printTargetHasOwnshipInRangeReward = false;
    [SerializeField] private bool m_printTargetChasesOwnshipReward = false;

    public void PrintTeamRewards() {
        EnemiesData enemiesData = sceneComponents.GetEnemiesData(m_selectedTeamRewards, JetAgent.GetTargetTeam(m_selectedTeamRewards));

        JetRewards jetRewards = SceneRewards.RequestJetReward(this, enemiesData);

        if (m_printTotalReward)
            Debug.Log("[TOTAL] Reward: " + jetRewards.total);

        if (m_printOwnshipInboundReward)
            Debug.Log("[NEUTRAL] Altitude Reward: " + jetRewards.inbound);
        
        if (m_printOwnshipAdvantageBehindTargetReward)
            Debug.Log("[ADVANTAGE] Position Advantage Over Target Reward: " + jetRewards.ownshipAdvantageBehindTarget);
        if (m_printOwnshipHasTargetInRangeReward)
            Debug.Log("[ADVANTAGE] Target In Range Reward: " + jetRewards.ownshipHasTargetInRange);
        if (m_printOwnshipChasesTargetReward)
            Debug.Log("[ADVANTAGE] Chase Target Reward: " + jetRewards.ownshipChasesTarget);

        if (m_printTargetAdvantageBehindOwnshipReward)
            Debug.Log("[DISADVANTAGE] Target Position Advantage Over Ownship Reward: " + jetRewards.targetAdvantageBehindOwnship);
        if (m_printTargetHasOwnshipInRangeReward)
            Debug.Log("[DISADVANTAGE] OWNSHIP In Range Reward: " + jetRewards.targetHasOwnshipInRange);
        if (m_printTargetChasesOwnshipReward)
            Debug.Log("[DISADVANTAGE] Target Chase Ownship Reward: " + jetRewards.targetChasesOwnship);
    }

    // Actions ----------------------------------------------------------------
    [SerializeField] private Team m_selectedTeamActions;
    [SerializeField] private bool m_printActionsEveryFrame = false;

    [SerializeField] private bool m_printRollAction = false;
    [SerializeField] private bool m_printPitchAction = false;
    [SerializeField] private bool m_printYawAction = false;
    [SerializeField] private bool m_printThrustAction = false;
    [SerializeField] private bool m_printFlapsAction = false;
    [SerializeField] private bool m_printLaunchMissileAction = false;
    [SerializeField] private bool m_printDropDecoyAction = false;

    private void PrintTeamActions() {
        JetAction jetData = sceneComponents.GetJetData(m_selectedTeamActions).GetJetAction();

        if (m_printRollAction)
            Debug.Log("[ROLL] Action: " + jetData.GetRollAction());
        if (m_printPitchAction)
            Debug.Log("[PITCH] Action: " + jetData.GetPitchAction());
        if (m_printYawAction)
            Debug.Log("[YAW] Action: " + jetData.GetYawAction());
        if (m_printThrustAction)
            Debug.Log("[THRUST] Action: " + jetData.GetThrustAction());
        if (m_printFlapsAction)
            Debug.Log("[FLAPS] Action: " + jetData.GetFlapsAction());
        if (m_printLaunchMissileAction)
            Debug.Log("[LAUNCH MISSILE] Action: " + jetData.GetLaunchMissileAction());
        if (m_printDropDecoyAction)
            Debug.Log("[DROP DECOY] Action: " + jetData.GetDropDecoyAction());
    }
}

public class EnemiesData {
    public float distance;

    public float angleOwnshipToTarget;
    public float angleOwnshipBehindTarget;

    public float angleTargetToOwnship;
    public float angleTargetBehindOwnship;

    public bool ownshipIncomingMissile;
    public Team ownshipTeam;
    public JetData ownshipJetData;
    public JetInputs ownshipJetInputs;
    public JetAction ownshipJetAction;
    public GameObject ownshipJetObject;
    public AircraftController ownshipAircraftController;

    public bool targetIncomingMissile;
    public Team targetTeam;
    public JetData targetJetData;
    public JetInputs targetJetInputs;
    public JetAction targetJetAction;
    public GameObject targetJetObject;
    public AircraftController targetAircraftController;

    public EnemiesData(JetData ownshipJetData, JetData targetJetData) {
        ownshipJetObject = ownshipJetData.GetObject();
        ownshipAircraftController = ownshipJetObject.GetComponent<AircraftController>();
        ownshipTeam = ownshipJetData.GetTeam();
        this.ownshipJetData = ownshipJetData;

        targetJetObject = targetJetData.GetObject();
        targetAircraftController = targetJetObject.GetComponent<AircraftController>();
        targetTeam = targetJetData.GetTeam();
        this.targetJetData = targetJetData;
    }

    public void UpdateDataAfterAction() {
        Vector3 ownshipTargetVector = targetJetObject.transform.position - ownshipJetObject.transform.position;

        distance = Vector3.Distance(ownshipJetObject.transform.position, targetJetObject.transform.position);

        angleOwnshipToTarget = Vector3.Angle(ownshipTargetVector, ownshipJetObject.transform.forward);
        angleOwnshipBehindTarget = Vector3.Angle(-ownshipTargetVector, -targetJetObject.transform.forward);

        angleTargetToOwnship = Vector3.Angle(-ownshipTargetVector, targetJetObject.transform.forward);
        angleTargetBehindOwnship = Vector3.Angle(ownshipTargetVector, -ownshipJetObject.transform.forward);

        ownshipJetInputs = ownshipJetData.GetJetInputs();
        ownshipJetAction = ownshipJetData.GetJetAction();

        targetJetInputs = targetJetData.GetJetInputs();
        targetJetAction = targetJetData.GetJetAction();
    }

    public void UpdateDataForNextAction() {
        ownshipIncomingMissile = (ownshipAircraftController.GetIncomingMissileTransform() != null);
        targetIncomingMissile = (targetAircraftController.GetIncomingMissileTransform() != null);
    }
}
