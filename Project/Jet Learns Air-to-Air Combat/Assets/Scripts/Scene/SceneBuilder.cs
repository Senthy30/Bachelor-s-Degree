using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBuilder {

    // scene builder data
    private int m_idScene;

    private SceneConfig m_sceneConfig;
    private SceneData m_sceneData;
    private GameObject m_sceneObject;

    // parent transform objects
    private Transform m_boxParentTransform;
    private Transform m_waterParentTransform;
    private Transform m_aircraftCarrierParentTransform;
    private Transform m_jetsParentTransform;
    private Transform m_missileParentTransform;
    private Transform m_decoyParentTransform;

    // built data

    private SceneComponents m_sceneComponents;

    public SceneBuilder(int idScene, GameObject sceneObject, SceneConfig sceneConfig) {
        m_idScene = idScene;
        m_sceneConfig = sceneConfig;
        m_sceneObject = sceneObject;
        m_sceneData = sceneObject.GetComponent<SceneData>();

        Debug.Assert(m_sceneData != null, "SceneSettings not found in SceneBuilder");

        BuildScene();
    }

    public void RebuildScene() {
        RebuildComponents();
    }

    private void BuildScene() {
        FindParentsObjects();

        BuildComponents();

        SetSceneComponentsBuilt();
    }

    private void BuildComponents() {
        AircraftCarrierData.ClearListOfValidAircraftCarrierCoords();
        m_sceneComponents = new SceneComponents(m_sceneConfig, m_sceneData);

        foreach (Team team in System.Enum.GetValues(typeof(Team))) {
            AircraftCarrierData aircraftCarrierData = BuildAircraftCarrier(team);
            JetData jetData = BuildJet(team, aircraftCarrierData.GetTransformJetSpawnPoint());

            jetData.SetNumDecoys(m_sceneConfig.numDecoysPerJet);
            m_sceneComponents.AddJetData(jetData);
            m_sceneComponents.AddAircraftCarrierData(aircraftCarrierData);
            m_sceneComponents.AddHeatEmission(new HeatEmission(jetData.GetObject().transform, 1));
        }

        m_sceneComponents.SetEnemyChunksData(BuildEnemyChunks());
        m_sceneComponents.SetBoxData(BuildBox());
        m_sceneComponents.SetWaterData(BuildWater());
        m_sceneComponents.SetMissilePhysicsHeatEmission();
        m_sceneComponents.SetJetMissilesSceneParentObject(m_missileParentTransform);
    }

    private void RebuildComponents() {
        if (m_sceneComponents == null) {
            Debug.LogError("SceneComponents not built");
            return;
        }

        m_sceneComponents.ClearMissiles();
        m_sceneComponents.ClearHeatEmission();
        AircraftCarrierData.ClearListOfValidAircraftCarrierCoords();

        foreach (Team team in System.Enum.GetValues(typeof(Team))) { 
            RebuildAircraftCarrier(team);
            RebuildJet(team);
        }

        DeleteAllDecoys();
        DeleteAllMissiles();
        m_sceneComponents.SetMissilePhysicsHeatEmission();
        m_sceneComponents.SetJetMissilesSceneParentObject(m_missileParentTransform);
        Object.FindObjectOfType<TheaterData>().ChangeViewModeIfViewMissileIsActive();
    }

    private void SetSceneComponentsBuilt() {
        m_sceneData.SetSceneComponents(m_sceneComponents);
    }

    private AircraftCarrierData BuildAircraftCarrier(Team team) {
        return new AircraftCarrierData(m_idScene, team, m_aircraftCarrierParentTransform);
    }

    private void RebuildAircraftCarrier(Team team) {
        m_sceneComponents.GetAircraftCarrierData(team).Rebuild();
    }

    private JetData BuildJet(Team team, Transform jetSpawnTransform) {
        return new JetData(team, jetSpawnTransform, m_jetsParentTransform, m_sceneObject, m_sceneComponents); ;
    }

    private void RebuildJet(Team team) {
        JetData jetData = m_sceneComponents.GetJetData(team);

        jetData.Rebuild();
        jetData.SetNumDecoys(m_sceneConfig.numDecoysPerJet);
        m_sceneComponents.AddHeatEmission(new HeatEmission(jetData.GetObject().transform, 1));
    }

    private BoxData BuildBox() {
        return new BoxData(m_sceneObject);
    }

    private WaterData BuildWater() {
        return new WaterData(m_sceneObject);
    }

    private EnemyChunksData BuildEnemyChunks() {
        return new EnemyChunksData(m_sceneObject);
    }

    public DecoyData BuildDecoy(Vector3 position) {
        DecoyData decoyData = new DecoyData(m_sceneConfig.decoyPrefab, m_decoyParentTransform, position, m_sceneData);
        HeatEmission heatEmission = new HeatEmission(decoyData.GetObject().transform, 2);

        m_sceneComponents.AddHeatEmission(heatEmission);
        decoyData.SetHeatEmission(heatEmission);

        return decoyData;
    }

    private void DeleteAllDecoys() {
        foreach (Transform child in m_decoyParentTransform) {
            Object.Destroy(child.gameObject);
        }
    }

    private void DeleteAllMissiles() {
        foreach (Transform child in m_missileParentTransform) {
            Object.Destroy(child.gameObject);
        }
    }

    private void FindParentsObjects() {
        Transform transform = m_sceneObject.transform;

        m_jetsParentTransform = transform.Find(m_sceneConfig.nameJetParentObject).transform;
        m_aircraftCarrierParentTransform = transform.Find(m_sceneConfig.nameAircraftCarrierParentObject).transform;
        m_missileParentTransform = transform.Find(m_sceneConfig.nameMissileParentObject).transform;
        m_decoyParentTransform = transform.Find(m_sceneConfig.nameDecoyParentObject).transform;
        m_waterParentTransform = transform.Find(m_sceneConfig.nameWaterParentObject).transform;
        m_boxParentTransform = transform.Find(m_sceneConfig.nameBoxParentObject).transform;
    }

}
