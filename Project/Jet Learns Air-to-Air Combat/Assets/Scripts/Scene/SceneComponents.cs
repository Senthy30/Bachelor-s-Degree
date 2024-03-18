using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneComponents {

    private SceneConfig m_sceneConfig;

    private BoxData m_boxData;
    private WaterData m_waterData;
    private EnemyChunksData m_enemyChunksData;

    private List<AircraftCarrierData> m_instancesAircraftCarrierData = new List<AircraftCarrierData>();
    private List<JetData> m_instancesJetData = new List<JetData>();
    private List<HeatEmission> m_instancesHeatEmission = new List<HeatEmission>();

    public SceneComponents(SceneConfig sceneConfig) {
        m_sceneConfig = sceneConfig;
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

    // Setters ------------------------------------------------

    public void SetMissilePhysicsHeatEmission() {
        for (int i = 0; i < m_instancesJetData.Count; i++) {
            m_instancesJetData[i].SetMissilePhysicsHeatEmission(m_instancesHeatEmission);
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

    // MissilePhysics ------------------------------------------

    public void TriggerMissilesFindTarget() {
        for (int i = 0; i < m_instancesJetData.Count; i++) {
            if (m_instancesJetData[i] != null && m_instancesJetData[i].GetObject() != null)
                m_instancesJetData[i].TriggerMissilesFindTarget();
        }
    }

    // Remove methods -----------------------------------------

    public void RemoveHeatEmission(HeatEmission heatEmission) {
        m_instancesHeatEmission.Remove(heatEmission);
    }

    // Clear --------------------------------------------------

    public void Clear() {
        m_instancesAircraftCarrierData.Clear();
        m_instancesJetData.Clear();
        m_instancesHeatEmission.Clear();
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
