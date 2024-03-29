using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Scenario {
    TAKE_OFF,
    BLUE_DEFENDING,
    RED_DEFENDING,
    FLIGHTING,
    FINAL
}

public class SceneData : MonoBehaviour {

    private static int nextIdScene = 0;
    private static float rebuildSceneAfterSeconds = 5.0f;

    private bool rebuildingScene;
    private int idScene;
    private int numJetsExploded = 0;
    private Scenario scenario;

    [SerializeField] private SceneConfig sceneConfig;
    private SceneBuilder sceneBuilder = null;
    private SceneComponents sceneComponents = null;

    void Awake() {
        BuildScene();
    }

    void Update() {
        if (m_printEveryFrame)
            PrintTeamInputs();
    }

    public void BuildScene() {
        idScene = nextIdScene++;
        numJetsExploded = 0;
        rebuildingScene = false;

        SetResolutionSceneConfig();
        SceneComponents.SetSceneConfig(sceneConfig);

        sceneBuilder = new SceneBuilder(idScene, gameObject, sceneConfig);
    }

    public void DropDecoy(Team team) {
        JetData jetData = sceneComponents.GetJetData(team);

        if (jetData.GetNumDecoys() > 0) {
            jetData.DecrementNumDecoys();

            DecoyData decoyData = sceneBuilder.BuildDecoy(jetData.GetDecoySpawnPointTransform().position);
            DecoyPhysics decoyPhysics = decoyData.GetObject().GetComponent<DecoyPhysics>();

            decoyPhysics.AddJetCollision(jetData.GetColliderParentObject());
            decoyPhysics.SetVelocity(jetData.GetObject().GetComponent<Rigidbody>().velocity);
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
            StartCoroutine(CallRebuildSceneAfterSeconds(rebuildSceneAfterSeconds));
        }
    }

    IEnumerator CallRebuildSceneAfterSeconds(float delay) {
        yield return new WaitForSeconds(delay);

        numJetsExploded = 0;
        sceneBuilder.RebuildScene();
        rebuildingScene = false;
    }

    // Setters ------------------------------------------------

    public void SetSceneComponents(SceneComponents sceneComponents) {
        this.sceneComponents = sceneComponents;
    }

    public void SetResolutionSceneConfig() {
        if (sceneConfig.resolution != TheaterData.GetResolution())
            sceneConfig.resolution = TheaterData.GetResolution();
    }

    public static void SetNextIdScene(int id) {
        nextIdScene = id;
    }

    public static void SetRebuildSceneAfterSeconds(float seconds) {
        rebuildSceneAfterSeconds = seconds;
    }

    // Getters ------------------------------------------------

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
    [SerializeField] private Team m_selectedTeam;
    [SerializeField] private bool m_printEveryFrame = false;

    [SerializeField] private bool m_printOwnshipDecoys = false;
    [SerializeField] private bool m_printOwnshipMissiles = false;
    [SerializeField] private bool m_printOwnshipPitch = false;
    [SerializeField] private bool m_printOwnshipRoll = false;
    [SerializeField] private bool m_printOwnshipYaw = false;
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
        JetInputs jetInputs = SceneInputs.RequestJetInputs(this, m_selectedTeam);

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
            Debug.Log("[DECOYS] In Range Relative Position: " + jetInputs.inRangeDecoyRelativePosition);

        if (m_printLaunchedMissileDistanceToTarget)
            Debug.Log("[MISSILE] Launched Missile Distance To Target: " + jetInputs.launchedMissileDistanceToTarget);
        if (m_printIncomingMissileDistanceToOwnship)
            Debug.Log("[MISSILE] Incoming Missile Distance To Ownship: " + jetInputs.incomigMissileDistanceToOwnship);
        if (m_printIncomingMissileRelativeDirection)
            Debug.Log("[MISSILE] Incoming Missile Relative Position: " + jetInputs.incomingMissileRelativeDirection);
    }
}
