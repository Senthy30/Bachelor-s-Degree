using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheaterBuilder {

    private static bool editorMode = false;
    private static bool rebuildScene = false;
    private static Vector3 offsetScenes = new Vector3(1f, 5f, 1f);

    private Vector3Int m_numScenes;
    private Vector3 m_size;
    private Vector3 m_centerFirstScene;

    private SceneConfig m_sceneConfig;
    private TheaterComponents m_theaterComponents;
    private TheaterPhysicsCalculation m_theaterPhysicsCalculation;
    private GameObject m_theaterObject;
    private GameObject m_sceneObjectPrefab;

    public TheaterBuilder(Vector3Int numScenes, GameObject theaterObject, GameObject sceneObjectPrefab) {
        m_numScenes = numScenes;
        m_theaterObject = theaterObject;
        m_sceneObjectPrefab = sceneObjectPrefab;
        m_sceneConfig = sceneObjectPrefab.GetComponent<SceneData>().GetSceneConfig();
        m_theaterPhysicsCalculation = theaterObject.GetComponent<TheaterPhysicsCalculation>();
        m_size = CalculateSizeTheater();
        m_centerFirstScene = CalculateCenterFirstScene();
        m_theaterComponents = new TheaterComponents();

        BuildTheater();
        SetTheaterComponentsBuilt();
    }

    private void BuildTheater() {
        int currentSceneToBuild = 0;

        DestroyPreviousScenes();
        for (int y = 0; y < m_numScenes.y; y++) {
            for (int x = 0; x < m_numScenes.x; x++) {
                for (int z = 0; z < m_numScenes.z; z++, currentSceneToBuild++) {
                    Vector3 positionScene = CalculatePositionScene(x, y, z);
                    BuildScene(currentSceneToBuild, positionScene);
                }
            }
        }
    }

    private void BuildScene(int currentSceneToBuild, Vector3 positionScene) {
        GameObject sceneObject = InstantiateScene(currentSceneToBuild, positionScene);
        SceneData sceneData = sceneObject.GetComponent<SceneData>();

        if (editorMode) {
            sceneData.BuildScene();

            /* [TODO] add serialization
            if (s_rebuildScene) {
                foreach (Team team in System.Enum.GetValues(typeof(Team))) {
                    AircraftCarrierData aircraftCarrierData = sceneComponents.GetAircraftCarrierData(team);
                    Vector3 positionAircraftCarrier = aircraftCarrierData.GetPosition();
                    Quaternion rotationAircraftCarrier = aircraftCarrierData.GetRotation();

                    theaterAircraftsCarrierData.GetSceneAircraftsCarrierData(currentSceneToBuild).SetPositionAndRotationByTeam(team, transformValues.Item1, transformValues.Item2);
                    theaterAircraftsCarrierData.GetSceneAircraftsCarrierData(currentSceneToBuild).SetPositionAndRotationByTeam(team, transformValues.Item1, transformValues.Item2);
                }

                SerializeTheaterAircraftsCarrierDataInFile();
            }
            */
        }

        SceneComponents sceneComponents = sceneData.GetSceneComponents();
        List<GameObject> instancesJetData = sceneComponents.GetJetData().ConvertAll(jetData => jetData.GetObject());
        for (int index = 0; index < instancesJetData.Count; index++) {
            int indexForPhysicsCalculationArray = m_theaterPhysicsCalculation.AddAircraftPhysics(instancesJetData[index].GetComponent<AircraftPhysics>());
            sceneComponents.GetJetData(index).SetPhysicsCalculationArrayIndex(indexForPhysicsCalculationArray);
        }

        m_theaterComponents.AddScene(sceneData);
    }

    private GameObject InstantiateScene(int currentSceneToBuild, Vector3 positionScene) {
        GameObject sceneObject = Object.Instantiate(
            m_sceneObjectPrefab, positionScene, Quaternion.Euler(Vector3.zero), m_theaterObject.transform
        );
        sceneObject.name = "Scene " + currentSceneToBuild;

        return sceneObject;
    }

    private void DestroyPreviousScenes() {
        m_theaterPhysicsCalculation.ClearAircraftPhysics();
        while (m_theaterObject.transform.childCount > 0)
            GameObject.DestroyImmediate(m_theaterObject.transform.GetChild(0).gameObject);
    }

    private void SetTheaterComponentsBuilt() {
        m_theaterObject.GetComponent<TheaterData>().SetTheaterComponents(m_theaterComponents);
    }

    // Calculations ------------------------------------------------

    private Vector3 CalculatePositionScene(int x, int y, int z) {
        return new Vector3(
            m_centerFirstScene.x + x * (m_sceneConfig.sceneSize.x + offsetScenes.x) * m_sceneConfig.GetSizePlane(),
            m_centerFirstScene.y + y * (m_sceneConfig.sceneSize.y + offsetScenes.y) * m_sceneConfig.GetSizePlane(),
            m_centerFirstScene.z + z * (m_sceneConfig.sceneSize.z + offsetScenes.z) * m_sceneConfig.GetSizePlane()
        );
    }

    private Vector3 CalculateSizeTheater() {
        return new Vector3(
            m_numScenes.x * (m_sceneConfig.sceneSize.x + offsetScenes.x),
            m_numScenes.y * (m_sceneConfig.sceneSize.y + offsetScenes.y),
            m_numScenes.z * (m_sceneConfig.sceneSize.z + offsetScenes.z)
        );
    }

    private Vector3 CalculateCenterFirstScene() {
        return new Vector3(
            m_sceneConfig.sceneSize.x / 2f - m_size.x / 2f,
            m_sceneConfig.sceneSize.y / 2f - m_size.y / 2f,
            m_sceneConfig.sceneSize.z / 2f - m_size.z / 2f
        );
    }

    // Static methods ----------------------------------------------

    public static void SetEditorMode(bool value) {
        editorMode = value;
    }

}
