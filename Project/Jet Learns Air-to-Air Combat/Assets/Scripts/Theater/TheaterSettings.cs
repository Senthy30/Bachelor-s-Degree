using Cinemachine;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TheaterSettings : MonoBehaviour {

    private const string PATH_SAVED_POSITION_ROTATION = "Assets/Data/Aircraft Carrier/theater_data.json";

    public static Resolution resolution;
    private static TheaterAircraftsCarrierData theaterAircraftsCarrierData;
    private static Vector3 offsetScenes = new Vector3(1f, 5f, 1f);

    [SerializeField]
    private bool rebuildScene = false;
    private static bool s_rebuildScene = false;
    [SerializeField]
    private Vector3Int numScenes;
    private int totalNumScenes;
    private int currentSceneToBuild;

    private int currentSceneView;
    private Team currentTeamView;
    private View currentView;

    [Header("Camera")]

    [SerializeField]
    private CinemachineFreeLook thirdPersonCinemachine = null;

    [Header("Scene")]

    [SerializeField]
    private GameObject scenePrefab;
    private TheaterPhysicsCalculation theaterPhysicsCalculation;

    private List<GameObject> sceneArray = new List<GameObject>();

    /*
    private void Awake() {
        BuildTheater();
    }

    private void Update() {
        HandleInputsEvent();
    }

    public void BuildTheater(bool editorMode = false) {
        DestroyPreviousScenes();
        SetDefaultSettings();

        theaterPhysicsCalculation = gameObject.GetComponent<TheaterPhysicsCalculation>();
        SceneConfig sceneConfig = scenePrefab.GetComponent<SceneData>().GetSceneConfig();
        Vector3 sizeTheater = new Vector3(
            numScenes.x * (sceneConfig.sceneSize.x + offsetScenes.x),
            numScenes.y * (sceneConfig.sceneSize.y + offsetScenes.y),
            numScenes.z * (sceneConfig.sceneSize.z + offsetScenes.z)
        );
        Vector3 centerOfFirstScene = new Vector3(
            sceneConfig.sceneSize.x / 2f - sizeTheater.x / 2f,
            sceneConfig.sceneSize.y / 2f - sizeTheater.y / 2f,
            sceneConfig.sceneSize.z / 2f - sizeTheater.z / 2f
        ) * sceneConfig.GetSizePlane();

        currentSceneToBuild = 0;
        for (int y = 0; y < numScenes.y; y++) {
            for (int x = 0; x < numScenes.x; x++) {
                for (int z = 0; z < numScenes.z; z++) {
                    Vector3 position = new Vector3(
                        centerOfFirstScene.x + x * (sceneConfig.sceneSize.x + offsetScenes.x) * sceneConfig.GetSizePlane(),
                        centerOfFirstScene.y + y * (sceneConfig.sceneSize.y + offsetScenes.y) * sceneConfig.GetSizePlane(),
                        centerOfFirstScene.z + z * (sceneConfig.sceneSize.z + offsetScenes.z) * sceneConfig.GetSizePlane()
                    );
                    BuildScene(position, editorMode);
                    ++currentSceneToBuild;
                }
            }
        }

        SetTransformOfCurrentView();
    }

    private void BuildScene(Vector3 position, bool editorMode) {
        GameObject scene = Instantiate(scenePrefab, position, Quaternion.Euler(Vector3.zero), transform);
        scene.name = "Scene " + currentSceneToBuild;

        SceneData sceneData = scene.GetComponent<SceneData>();
        SceneComponents sceneComponents = sceneData.GetSceneComponents();
        List <GameObject> instancesJetData = sceneComponents.GetJetData().ConvertAll(jetData => jetData.GetObject());

        foreach (GameObject jet in instancesJetData) {
            theaterPhysicsCalculation.AddAircraftPhysics(jet.GetComponent<AircraftPhysics>());
        }

        if (editorMode) {
            sceneData.BuildScene();

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
        }

        sceneArray.Add(scene);
    }

    private void ClearTheaterData() {
        sceneArray.Clear();
    }

    private void DestroyPreviousScenes() {
        ClearTheaterData();
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }

    private void SetDefaultSettings() {
        s_rebuildScene = rebuildScene;
        currentSceneView = 0;
        currentTeamView = (Team)0;
        currentView = View.THIRD_PERSON;
        totalNumScenes = numScenes.x * numScenes.y * numScenes.z;
        sceneArray = new List<GameObject>(totalNumScenes);
        SceneSettings.SetNextIdScene(0);
        resolution = scenePrefab.GetComponent<SceneSettings>().GetSceneConfig().resolution;
        theaterAircraftsCarrierData = new TheaterAircraftsCarrierData(totalNumScenes, GetNumTeams());
        if (!s_rebuildScene) {
            DeserializeTheaterAircraftsCarrierDataFromFile();
        }
    }

    private void HandleInputsEvent() {
        HandleSceneNavigationInputsEvent();
        HandleTeamNavigationInputsEvent();
    }

    private void HandleSceneNavigationInputsEvent() {

    }

    private void HandleTeamNavigationInputsEvent() {
        // convert value to 
        bool wasPressed = false;
        int currentTeamValue = (int)currentTeamView;
        
        if (Input.GetKeyDown(KeyCode.P)) {
            wasPressed = true;
            ++currentTeamValue;
            if (currentTeamValue >= GetNumTeams())
                currentTeamValue = 0;
        } else if (Input.GetKeyDown(KeyCode.O)) {
            wasPressed = true;
            --currentTeamValue;
            if (currentTeamValue < 0)
                currentTeamValue = GetNumTeams() - 1;
        }

        if (wasPressed) {
            SetTransformOfCurrentView();
            currentTeamView = (Team)currentTeamValue;
        }
    }

    private void SetTransformOfCurrentView() {
        switch (currentView) {
            case View.THIRD_PERSON:
                thirdPersonCinemachine.m_Follow = GetTransformOfCurrentView();
                thirdPersonCinemachine.m_LookAt = GetTransformOfCurrentView();
                break;
        }
    }

    private Transform GetTransformOfCurrentView() {
        switch (currentView) {
            case View.THIRD_PERSON:
                return sceneArray[currentSceneView].GetComponent<SceneSettings>().GetThirdPersonViewTeam(currentTeamView);
            default:
                return null;
        }
    }

    private void SerializeTheaterAircraftsCarrierDataInFile() {
        using (StreamWriter write = new StreamWriter(PATH_SAVED_POSITION_ROTATION)) {
            string serializedObject = JsonUtility.ToJson(theaterAircraftsCarrierData);
            write.Write(serializedObject);
        }
    }

    private void DeserializeTheaterAircraftsCarrierDataFromFile() {
        using (StreamReader reader = new StreamReader(PATH_SAVED_POSITION_ROTATION)) {
            string serializedObject = reader.ReadToEnd();
            theaterAircraftsCarrierData = JsonUtility.FromJson<TheaterAircraftsCarrierData>(serializedObject);
        }
    }

    public static int GetNumTeams() {
        return System.Enum.GetValues(typeof(Team)).Length;
    }

    public static bool GetRebuildScene() {
        return s_rebuildScene;
    }

    public static Vector3 GetSavedAircraftPositionsByTeam(int idScene, Team team) {
        return theaterAircraftsCarrierData.GetSceneAircraftsCarrierData(idScene).GetPositionByTeam(team);
    }

    public static Quaternion GetSavedAircraftRotationByTeam(int idScene, Team team) {
        return theaterAircraftsCarrierData.GetSceneAircraftsCarrierData(idScene).GetRotationByTeam(team);
    }
    */
}

public enum Resolution { LOW_RESOLUTION, HIGH_RESOLUTION };
public enum Component { WATER, JET, AIRCRAFT_CARRIER };
public enum Team {
    BLUE = 0,
    RED = 1
};
public enum View { FIRST_PERSON, THIRD_PERSON, OVERVIEW };