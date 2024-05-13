using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneComponents {

    private static TheaterData theaterData = null;

    private int m_currentMissileDataViewIndex = 0;
    private MissileData m_currentMissileDataView;

    private SceneConfig m_sceneConfig;
    private SceneData m_sceneData;

    private BoxData m_boxData;
    private WaterData m_waterData;
    private EnemyChunksData m_enemyChunksData;

    private List<AircraftCarrierData> m_instancesAircraftCarrierData = new List<AircraftCarrierData>();
    private List<JetData> m_instancesJetData = new List<JetData>();
    private List<MissileData> m_unlaunchedMissilesData = new List<MissileData>();
    private List<MissileData> m_launchedMissilesData = new List<MissileData>();
    private List<DecoyData> m_dropedDecoysData = new List<DecoyData>();
    private List<HeatEmission> m_instancesHeatEmission = new List<HeatEmission>();

    private List<EnemiesData> enemiesData = null;

    public SceneComponents(SceneConfig sceneConfig, SceneData sceneData) {
        m_sceneConfig = sceneConfig;
        m_sceneData = sceneData;
        if (theaterData == null)
            theaterData = Object.FindObjectOfType<TheaterData>();
    }

    // Add methods --------------------------------------------

    public void AddAircraftCarrierData(AircraftCarrierData aircraftCarrierData) {
        m_instancesAircraftCarrierData.Add(aircraftCarrierData);
    }

    public void AddJetData(JetData jetData) {
        m_instancesJetData.Add(jetData);
    }

    public void AddHeatEmission(HeatEmission heatEmission) {
        m_instancesHeatEmission.Add(heatEmission);
    }

    public void AddUnlaunchedMissileData(MissileData missileData) {
        m_unlaunchedMissilesData.Add(missileData);
    }

    public SceneConfig GetSceneConfig() {
        return m_sceneConfig;
    }

    // Setters ------------------------------------------------

    public void SetMissilePhysicsHeatEmission() {
        for (int i = 0; i < m_instancesJetData.Count; i++) {
            m_instancesJetData[i].SetMissilePhysicsHeatEmission(m_instancesHeatEmission);
        }
    }

    public void SetJetMissilesSceneParentObject(Transform missileParentTransform) {
        for (int i = 0; i < m_instancesJetData.Count; i++) {
            m_instancesJetData[i].SetMissilesSceneParentObject(missileParentTransform);
        }
    }

    public void SetBoxData(BoxData boxData) {
        m_boxData = boxData;
    }

    public void SetWaterData(WaterData waterData) {
        m_waterData = waterData;
    }

    public void SetEnemyChunksData(EnemyChunksData enemyChunksData) {
        m_enemyChunksData = enemyChunksData;
    }

    // Getters ------------------------------------------------
    // AircraftCarrierData ------------------------------------

    public List<AircraftCarrierData> GetAircraftCarrierData() {
        return m_instancesAircraftCarrierData;
    }

    public AircraftCarrierData GetAircraftCarrierData(int index) {
        return m_instancesAircraftCarrierData[index];
    }

    public AircraftCarrierData GetAircraftCarrierData(Team team) {
        return m_instancesAircraftCarrierData[(int)team];
    }

    // JetData -------------------------------------------------

    public List<JetData> GetJetData() {
        return m_instancesJetData;
    }

    public JetData GetJetData(int index) {
        return m_instancesJetData[index];
    }

    public JetData GetJetData(Team team) {
        return m_instancesJetData[(int)team];
    }

    public bool AreAllJetsDestroyed() {
        for (int i = 0; i < m_instancesJetData.Count; i++) {
            if (m_instancesJetData[i] != null && m_instancesJetData[i].GetObject() != null && m_instancesJetData[i].GetObject().activeSelf)
                return false;
        }

        return true;
    }

    // HeatEmission --------------------------------------------

    public List<HeatEmission> GetHeatEmission() {
        return m_instancesHeatEmission;
    }

    public HeatEmission GetHeatEmission(int index) {
        return m_instancesHeatEmission[index];
    }

    // ThirdPersonView -----------------------------------------

    public Transform GetFirstPersonViewTeam(Team team) {
        return GetJetData(team).GetFirstPersonViewTransform();
    }

    public Transform GetThirdPersonViewTeam(Team team) {
        return GetJetData(team).GetThirdPersonViewTransform();
    }

    // MissileData ------------------------------------------

    public int GetUnlaunchedMissilesCount() {
        return m_unlaunchedMissilesData.Count;
    }

    public int GetLaunchedMissilesCount() {
        return m_launchedMissilesData.Count;
    }

    public MissileData GetMissileDataCurrentView() {
        if (m_launchedMissilesData.Count == 0)
            return null;

        for (int i = 0; i < m_launchedMissilesData.Count; i++) {
            if (m_launchedMissilesData[i] == m_currentMissileDataView) {
                m_currentMissileDataViewIndex = i;
                return m_launchedMissilesData[i];
            }
        }

        m_currentMissileDataViewIndex = Mathf.Min(m_currentMissileDataViewIndex, m_launchedMissilesData.Count - 1);
        m_currentMissileDataView = m_launchedMissilesData[m_currentMissileDataViewIndex];

        return m_currentMissileDataView;
    }

    public MissileData GetNSetMissileDataViewAtOffset(int offset) {
        if (m_launchedMissilesData.Count == 0)
            return null;

        offset = Mathf.Clamp(offset, -m_launchedMissilesData.Count, m_launchedMissilesData.Count);
        m_currentMissileDataViewIndex = (m_currentMissileDataViewIndex + offset + m_launchedMissilesData.Count) % m_launchedMissilesData.Count;
        m_currentMissileDataView = m_launchedMissilesData[m_currentMissileDataViewIndex];

        return m_currentMissileDataView;
    }

    public List<MissileData> GetUnlaunchedMissilesData() {
        return m_unlaunchedMissilesData;
    }

    public List<MissileData> GetLaunchedMissilesData() {
        return m_launchedMissilesData;
    }

    public void AddLaunchedMissileData(MissileData missileData) {
        m_launchedMissilesData.Add(missileData);
    }

    public void TriggerMissilesFindTarget() {
        for (int i = 0; i < m_instancesJetData.Count; i++) {
            if (m_instancesJetData[i] != null && m_instancesJetData[i].GetObject() != null)
                m_instancesJetData[i].TriggerMissilesFindTarget();
        }
    }

    public void OnMissileLaunched(MissileData missileData) {
        m_unlaunchedMissilesData.Remove(missileData);
        m_launchedMissilesData.Add(missileData);
    }

    public void OnMissileDestroy(MissileData missileData) {
        theaterData.MissileExploded(missileData.GetMissilePhysics().transform);
        m_launchedMissilesData.Remove(missileData);
    }

    // DecoyData --------------------------------------------

    public void AddDecoyData(DecoyData decoyData) {
        m_dropedDecoysData.Add(decoyData);
    }

    // Box --------------------------------------------------

    public BoxData GetBoxData() {
        return m_boxData;
    }

    // Enemies ------------------------------------------------

    public void BuildEnemiesData() {
        enemiesData = new List<EnemiesData> ();
        foreach (Team ownshipTeam in System.Enum.GetValues(typeof(Team))) {
            foreach (Team targetTeam in System.Enum.GetValues(typeof(Team))) {
                if (ownshipTeam == targetTeam) {
                    enemiesData.Add(null);
                    continue;
                }

                JetData ownshipJetData = GetJetData(ownshipTeam);
                JetData targetJetData = GetJetData(targetTeam);

                enemiesData.Add(new EnemiesData(ownshipJetData, targetJetData));
            }
        }
    }

    public EnemiesData GetEnemiesData(Team ownshipTeam, Team targetTeam) {
        int numTeams = TheaterData.GetNumTeams();
        int ownshipTeamValue = (int)ownshipTeam;
        int targetTeamValue = (int)targetTeam;

        return enemiesData[ownshipTeamValue * numTeams + targetTeamValue];
    }

    public EnemyChunksData GetEnemyChunksData() {
        return m_enemyChunksData;
    }

    // Remove methods -----------------------------------------

    public void RemoveHeatEmission(HeatEmission heatEmission) {
        m_instancesHeatEmission.Remove(heatEmission);
    }

    public void RemoveDecoyData(DecoyData decoyData) {
        m_dropedDecoysData.Remove(decoyData);
    }

    public List<DecoyData> GetDropedDecoyData() {
        return m_dropedDecoysData;
    }

    // Clear --------------------------------------------------

    public void Clear() {
        m_instancesAircraftCarrierData.Clear();
        m_instancesJetData.Clear();
        m_instancesHeatEmission.Clear();
    }

    public void ClearMissiles() {
        m_unlaunchedMissilesData.Clear();
        m_launchedMissilesData.Clear();
    }

    public void ClearHeatEmission() {
        m_instancesHeatEmission.Clear();
    }

    public void ClearDecoys() {
        m_dropedDecoysData.Clear();
    }

    // Static methods ------------------------------------------
    
    public static void SetSceneConfig(SceneConfig sceneConfig) {
        JetData.SetSceneConfig(sceneConfig);
        JetCollision.SetSceneConfig(sceneConfig);
        AircraftCarrierData.SetSceneConfig(sceneConfig);
        BoxData.SetSceneConfig(sceneConfig);
        WaterData.SetSceneConfig(sceneConfig);
        EnemyChunksData.SetSceneConfig(sceneConfig);
        DecoyPhysics.SetSceneConfig(sceneConfig);
    }
}