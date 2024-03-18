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

    private void BuildScene() {
        FindParentsObjects();

        BuildComponents();
        SetSceneComponentsBuilt();
    }

    private void BuildComponents() {
        AircraftCarrierData.ClearListOfValidAircraftCarrierCoords();
        m_sceneComponents = new SceneComponents(m_sceneConfig);

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
    }

    private void SetSceneComponentsBuilt() {
        m_sceneData.SetSceneComponents(m_sceneComponents);
    }

    private AircraftCarrierData BuildAircraftCarrier(Team team) {
        return new AircraftCarrierData(m_idScene, team, m_aircraftCarrierParentTransform);
    }

    private JetData BuildJet(Team team, Transform jetSpawnTransform) {
        JetData jetData = new JetData(team, jetSpawnTransform, m_jetsParentTransform, m_sceneObject);
        return jetData;
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

    private void FindParentsObjects() {
        Transform transform = m_sceneObject.transform;

        m_jetsParentTransform = transform.Find(m_sceneConfig.nameJetParentObject).transform;
        m_aircraftCarrierParentTransform = transform.Find(m_sceneConfig.nameAircraftCarrierParentObject).transform;
        m_missileParentTransform = transform.Find(m_sceneConfig.nameMissileParentObject).transform;
        m_decoyParentTransform = transform.Find(m_sceneConfig.nameDecoyParentObject).transform;
        m_waterParentTransform = transform.Find(m_sceneConfig.nameWaterParentObject).transform;
        m_boxParentTransform = transform.Find(m_sceneConfig.nameBoxParentObject).transform;
    }

    private Resolution GetResolution() {
        return m_sceneConfig.resolution;
    }

}
