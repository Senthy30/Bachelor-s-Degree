using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TheaterSettings : MonoBehaviour {

    const string PATH_SAVED_POSITION_ROTATION = "Assets/Data/Aircraft Carrier/theater_data.json";

    private static TheaterAircraftsCarrierData theaterAircraftsCarrierData;

    [SerializeField]
    private bool rebuildScene = false;
    private static bool s_rebuildScene = false;
    [SerializeField]
    private int numScenes;
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

    private List<GameObject> sceneArray = new List<GameObject>();

    private void Awake() {
        BuildTheater();
    }

    private void Update() {
        HandleInputsEvent();
    }

    public void BuildTheater(bool editorMode = false) {
        DestroyPreviousScenes();
        SetDefaultSettings();

        for (currentSceneToBuild = 0; currentSceneToBuild < numScenes; currentSceneToBuild++) {
            GameObject scene = Instantiate(
                scenePrefab, 
                Vector3.zero, 
                Quaternion.Euler(Vector3.zero),
                transform
            );

            if (editorMode) {
                scene.GetComponent<SceneSettings>().BuildScene();

                if (s_rebuildScene) {
                    SceneSettings sceneSettings = scene.GetComponent<SceneSettings>();

                    foreach (Team team in System.Enum.GetValues(typeof(Team))) {
                        Tuple <Vector3, Quaternion> transformValues = sceneSettings.GetAircraftCarrierPositionAndRotationByTeam(team);
                        theaterAircraftsCarrierData.GetSceneAircraftsCarrierData(currentSceneToBuild).SetPositionAndRotationByTeam(team, transformValues.Item1, transformValues.Item2); 
                        theaterAircraftsCarrierData.GetSceneAircraftsCarrierData(currentSceneToBuild).SetPositionAndRotationByTeam(team, transformValues.Item1, transformValues.Item2);
                    }

                    SerializeTheaterAircraftsCarrierDataInFile();
                }
            }

            sceneArray.Add(scene);
        }

        SetTransformOfCurrentView();
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
        sceneArray = new List<GameObject>(numScenes);
        SceneSettings.SetNextIdScene(0);
        theaterAircraftsCarrierData = new TheaterAircraftsCarrierData(numScenes, GetNumTeams());
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
}

public enum Resolution { LOW_RESOLUTION, HIGH_RESOLUTION };
public enum Component { WATER, JET, AIRCRAFT_CARRIER };
public enum Team {
    A = 0,
    B = 1,
    C = 2,
    D = 3,
    E = 4,
    F = 5,
    G = 6,
    H = 7,
    I = 8,
    J = 9,
    K = 10
};
public enum View { FIRST_PERSON, THIRD_PERSON, OVERVIEW };