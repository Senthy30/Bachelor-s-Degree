using System.Collections;
using System.Collections.Generic;
using Tarodev;
using UnityEngine;

public class JetData : IJetData {

    private static SceneConfig sceneConfig;
    private const float skipApplyForcesForSeconds = 0.5f;

    private int m_numDecoys = 0;
    private int m_indexForPhysicsCalculationArray = 0;
    private Team m_team;

    private AircraftPhysics m_aircraftPhysics;
    private JetCollision m_jetCollision;
    private JetInputs m_lastJetInputs;
    private JetInputs m_jetInputs;
    private JetRewards m_jetRewards;
    private JetAction m_jetAction;
    private List<MissileData> m_unlaunchedMissilesData;
    private List<MissileData> m_launchedMissilesData;

    private SceneComponents m_sceneComponents;

    private Transform m_jetSpawnTransform;
    private Transform m_firstPersonViewTransform;
    private Transform m_thirdPersonViewTransform;
    private Transform m_decoySpawnPointTransform;

    private GameObject m_object;

    public JetData(Team team, Transform jetSpawnTransform, Transform jetsParentTransform, GameObject sceneObject, SceneComponents sceneComponents) {
        m_team = team;
        m_jetSpawnTransform = jetSpawnTransform;
        m_object = JetDataUtils.InstantiateJetObject(team, m_jetSpawnTransform, jetsParentTransform, sceneConfig);

        m_aircraftPhysics = m_object.GetComponent<AircraftPhysics>();
        Debug.Assert(m_aircraftPhysics != null, "AircraftPhysics not found in JetData");

        m_jetCollision = new JetCollision(m_team, m_object, sceneObject.GetComponent<SceneData>());
        m_aircraftPhysics.AddFuncInOnCollision(m_jetCollision.OnCollisionEnter);

        m_sceneComponents = sceneComponents;

        m_unlaunchedMissilesData = new List<MissileData>();
        m_launchedMissilesData = new List<MissileData>();

        m_jetAction = new JetAction();
        m_jetInputs = new JetInputs();
        m_lastJetInputs = new JetInputs();

        BuildConfigJet(sceneObject);
    }

    public int GetNumDecoys() {
        return m_numDecoys;
    }

    public Team GetTeam() {
        return m_team;
    }

    public JetCollision GetJetCollision() {
        return m_jetCollision;
    }

    public JetAction GetJetAction() {
        return m_jetAction;
    }

    public JetInputs GetLastJetInputs() {
        return m_lastJetInputs;
    }

    public JetInputs GetJetInputs() {
        return m_jetInputs;
    }

    public JetRewards GetJetRewards() {
        return m_jetRewards;
    }

    public Transform GetFirstPersonViewTransform() {
        return m_firstPersonViewTransform;
    }

    public Transform GetThirdPersonViewTransform() {
        return m_thirdPersonViewTransform;
    }

    public Transform GetDecoySpawnPointTransform() {
        return m_decoySpawnPointTransform;
    }

    public GameObject GetObject() {
        return m_object;
    }

    public GameObject GetColliderParentObject() {
        return m_object.transform.Find(sceneConfig.collisionParentObjectName).gameObject;
    }

    public void DecrementNumDecoys() {
        m_numDecoys--;
    }

    public void SetNumDecoys(int numDecoys) {
        m_numDecoys = numDecoys;
    }

    public void SetJetAction(JetAction jetAction) {
        m_jetAction = jetAction;
    }

    public void SetLastJetInputs(JetInputs jetInputs) {
        m_lastJetInputs = jetInputs;
    }

    public void SetJetInputs(JetInputs jetInputs) {
        m_jetInputs = jetInputs;
    }

    public void SetJetRewards(JetRewards jetRewards) {
        m_jetRewards = jetRewards;
    }

    private void BuildConfigJet(GameObject sceneObject) {
        List <GameObject> missileArray = JetDataUtils.BuildMissileStorage(m_object, sceneConfig);
        JetDataUtils.SetLowNHighJetObjectActive(m_object, sceneConfig);
        JetDataUtils.SetLowNHighMissileStorageActive(m_object, sceneConfig);
        JetDataUtils.SetSceneDataForJet(m_team, this, m_object, sceneConfig, sceneObject, missileArray);
        m_unlaunchedMissilesData = new List<MissileData>();
        m_launchedMissilesData = new List<MissileData>();
        foreach (GameObject missileObject in missileArray) {
            missileObject.GetComponent<MissilePhysics>().CreateMissileData();
            m_unlaunchedMissilesData.Add(missileObject.GetComponent<MissilePhysics>().GetMissileData());
        }
        ConfigUnlaunchedMissiles();

        m_firstPersonViewTransform = m_object.transform.Find(sceneConfig.firstPersonViewName);
        m_thirdPersonViewTransform = m_object.transform.Find(sceneConfig.thirdPersonViewName);
        m_decoySpawnPointTransform = m_object.transform.Find(sceneConfig.decoySpawnPointName);

        m_object.name = m_team + " Jet";
        m_object.GetComponent<AircraftPhysics>().SkipApplyForcesFor(skipApplyForcesForSeconds);

        JetAgent jetAgent = m_object.GetComponent<JetAgent>();
        if (jetAgent != null) {
            jetAgent.SetJetData(this);
        }
    }

    public void Rebuild() {
        m_unlaunchedMissilesData.Clear();
        m_launchedMissilesData.Clear();

        m_object.SetActive(true);
        m_object.transform.position = m_jetSpawnTransform.position;
        m_object.transform.rotation = m_jetSpawnTransform.rotation;
        m_object.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_object.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_object.GetComponent<AircraftController>().ReconfigToInitialState();
        m_object.GetComponent<AircraftPhysics>().SkipApplyForcesFor(skipApplyForcesForSeconds);

        List <GameObject> missileArray = JetDataUtils.BuildMissileStorage(m_object, sceneConfig);
        JetDataUtils.SetLowNHighJetObjectActive(m_object, sceneConfig);
        JetDataUtils.SetLowNHighMissileStorageActive(m_object, sceneConfig);

        m_object.GetComponent<AircraftController>().AddMissilesInArray(missileArray);
        m_unlaunchedMissilesData = new List<MissileData>();
        m_launchedMissilesData = new List<MissileData>();
        foreach (GameObject missileObject in missileArray) {
            missileObject.GetComponent<MissilePhysics>().CreateMissileData();
            m_unlaunchedMissilesData.Add(missileObject.GetComponent<MissilePhysics>().GetMissileData());
        }
        ConfigUnlaunchedMissiles();

        Object.FindObjectOfType<TheaterPhysicsCalculation>().MarkAircraftPyhsicsForSkipping(m_indexForPhysicsCalculationArray, 0);
    }

    // Missile ------------------------------------------

    private void ConfigUnlaunchedMissiles() {
        for (int i = 0; i < m_unlaunchedMissilesData.Count; i++) {
            m_unlaunchedMissilesData[i].SetLaunchingTeam(m_team);
            m_unlaunchedMissilesData[i].GetMissilePhysics().AddFuncInActionOnLaunch(OnMissileLaunched);
            m_unlaunchedMissilesData[i].GetMissilePhysics().AddFuncInActionOnCollision(OnMissileDestroy);

            m_sceneComponents.AddUnlaunchedMissileData(m_unlaunchedMissilesData[i]);
        }
    }

    private void OnMissileLaunched(MissileData missileData) {
        m_sceneComponents.OnMissileLaunched(missileData);

        m_unlaunchedMissilesData.Remove(missileData);
        m_launchedMissilesData.Add(missileData);
    }

    private void OnMissileDestroy(MissileData missileData) {
        m_sceneComponents.OnMissileDestroy(missileData);

        m_launchedMissilesData.Remove(missileData);
    }

    public int GetNumUnlaunchedMissiles() {
        return m_unlaunchedMissilesData.Count;
    }

    public int GetNumLaunchedMissiles() {
        return m_launchedMissilesData.Count;
    }

    public void SetMissilesSceneParentObject(Transform missileParentTransform) {
        for (int i = 0; i < m_unlaunchedMissilesData.Count; i++) {
            m_unlaunchedMissilesData[i].GetMissilePhysics().SetMissileParentTransform(missileParentTransform);
        }
    }

    public void SetMissilePhysicsHeatEmission(List<HeatEmission> heatEmissionsArray) {
        for (int i = 0; i < m_unlaunchedMissilesData.Count; i++) {
            m_unlaunchedMissilesData[i].GetMissilePhysics().SetHeatEmissionArray(heatEmissionsArray);
        }
    }

    public void TriggerMissilesFindTarget() {
        for (int i = 0; i < m_launchedMissilesData.Count; i++) {
            m_launchedMissilesData[i].GetMissilePhysics().TriggerFindTarget();
        }
    }

    // Scenario ------------------------------------------

    public void SetNumUnlaunchedMissiles(int numMissiles) {
        while (m_unlaunchedMissilesData.Count > numMissiles) {
            m_unlaunchedMissilesData.RemoveAt(0);
        }
    }

    // Physics ------------------------------------------

    public void SetPhysicsCalculationArrayIndex(int index) {
        m_indexForPhysicsCalculationArray = index;
        m_jetCollision.SetPhysicsCalculationArrayIndex(index);
    }

    // Static methods ------------------------------------------

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }
}