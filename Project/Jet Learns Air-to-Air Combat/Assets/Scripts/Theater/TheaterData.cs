using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheaterData : MonoBehaviour {

    private const string PATH_SAVED_POSITION_ROTATION = "Assets/Data/Aircraft Carrier/theater_data.json";

    private static Resolution resolution;
    private static TheaterAircraftsCarrierData theaterAircraftsCarrierData;

    [SerializeField] private Resolution m_resolution;
    [SerializeField] private Scenario m_scenario;
    [SerializeField] private bool m_rebuildScene = false;
    [SerializeField] private Vector3Int m_numScenes;
    private int m_totalScenes;

    [Header("Camera")]
    private bool m_currentJetViewDestroyed = false;
    private bool m_currentMissileViewDestroyed = false;

    private int m_currentFirstPersonCinemachine;
    private int m_currentThirdPersonCinemachine;

    [SerializeField] private float m_changeViewIfCurrentJetDestroyedAfterTime = 2f;
    [SerializeField] private float m_changeViewIfCurrentMissileDestroyedAfterTime = 1f;

    [SerializeField] private List<CinemachineVirtualCamera> m_firstPersonCinemachinesArray = null;
    [SerializeField] private List<CinemachineFreeLook> m_thirdPersonCinemachinesArray = null;

    [SerializeField] private RewardsConfig m_rewardsConfig;

    [Header("Scene")]
    [SerializeField] private JetController m_jetController;
    [SerializeField] private GameObject m_scenePrefab;
    private GameObject m_lastJetViewObject;

    private int m_currentSceneView;
    private Team m_currentTeamView;
    private View m_lastView;
    private View m_currentView;
    private TheaterBuilder m_theaterBuilder;
    private TheaterComponents m_theaterComponents;
    private TheaterPhysicsCalculation m_theaterPhysicsCalculation;

    private void Awake() {
        BuildTheater(false);
        //Time.timeScale = 5f;
    }

    private void Start() {
        ConfigureDebugMode();
    }

    public void BuildTheater(bool editorMode) {
        DefaultTheaterConfiguration(editorMode);

        m_theaterBuilder = new TheaterBuilder(m_numScenes, gameObject, m_scenePrefab);

        ApplyCameraChanges();
        m_theaterPhysicsCalculation.CallAfterTheaterBuilt();
    }

    private void DefaultTheaterConfiguration(bool editorMode) {
        TheaterBuilder.SetEditorMode(editorMode);
        AircraftController.SetTheaterData(this);
        SceneData.SetNextIdScene(0);
        SceneRewards.rewardsConfig = m_rewardsConfig;

        resolution = m_resolution;
        m_totalScenes = m_numScenes.x * m_numScenes.y * m_numScenes.z;
        m_currentSceneView = 0;
        m_currentTeamView = (Team)0;
        m_currentView = View.THIRD_PERSON;
        m_theaterPhysicsCalculation = gameObject.GetComponent<TheaterPhysicsCalculation>();

        for (int i = 0; i < m_firstPersonCinemachinesArray.Count; i++) {
            m_firstPersonCinemachinesArray[i].Priority = 0;
        }

        for (int i = 0; i < m_thirdPersonCinemachinesArray.Count; i++) {
            m_thirdPersonCinemachinesArray[i].Priority = 0;
        }
    }

    public void ChangeViewMode() {
        if (m_currentView == View.FIRST_PERSON) {
            m_currentView = View.THIRD_PERSON;
        } else if (m_currentView == View.THIRD_PERSON) {
            m_currentView = View.FIRST_PERSON;
        } 

        ApplyCameraChanges();
    }

    public void ChangeViewModeToMissile() {
        if (m_currentView == View.MISSILE) {
            m_currentView = m_lastView;
            if (IsCurrentJetViewDestroyed() == false) {
                ApplyCameraChanges();
            } else {
                ChangeTeamWatching();
            }
        } else {
            if (HaveCurrentSceneLaunchedMissiles()) {
                m_lastView = m_currentView;
                m_currentView = View.MISSILE;
                ApplyCameraChanges();
            }
        }
    }

    public void ChangeViewModeIfViewMissileIsActive() {
        if (m_currentView == View.MISSILE) {
            ChangeViewModeToMissile();
        }
    }

    public void ChangeToSceneWatchingAtOffset(int offset) {
        int lastCurrentSceneView = m_currentSceneView;

        do {
            m_currentSceneView = (m_currentSceneView + offset + m_totalScenes) % m_totalScenes;
            if (lastCurrentSceneView == m_currentSceneView) {
                return;
            }
        } while (AreAllJetsInSceneViewDestroyed() == true);

        if (IsCurrentJetViewDestroyed() == true) {
            ChangeTeamWatching();
            return;
        }

        if (m_currentView != View.MISSILE) {
            ApplyCameraChanges();
        } else {
            ChangeViewModeToMissile();
        }
    }

    public void ChangeToMissileWatchingAtOffset(int offset) {
        if (m_currentView != View.MISSILE) {
            return;
        }

        SceneComponents sceneComponents = m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents();
        if (sceneComponents.GetLaunchedMissilesCount() == 0) {
            ChangeViewModeToMissile();
            return;
        }

        MissileData lastMissileData = sceneComponents.GetMissileDataCurrentView();
        MissileData nextMissileData = null;

        if (lastMissileData == null) {
            ChangeViewModeToMissile();
            return;
        }

        do {
            nextMissileData = sceneComponents.GetNSetMissileDataViewAtOffset(offset);
        } while (lastMissileData != nextMissileData && nextMissileData == null);

        if (nextMissileData == null) {
            ChangeViewModeToMissile();
            return;
        }

        ApplyCameraChanges();
    }

    public void ChangeTeamWatching() {
        int lastCurrentTeamView = (int)m_currentTeamView;

        do {
            m_currentTeamView = (Team)(((int)m_currentTeamView + 1) % GetNumTeams());
            if (lastCurrentTeamView == (int)m_currentTeamView) {
                if (IsCurrentJetViewDestroyed() == true)
                    ChangeToSceneWatchingAtOffset(1);
                return;
            }
        } while (IsCurrentJetViewDestroyed() == true);
        
        ApplyCameraChanges();
    }

    IEnumerator CallChangeTeamWatchingWithDelay(float delay) {
        yield return new WaitForSeconds(delay);

        m_currentJetViewDestroyed = false;
        ChangeTeamWatching();
    }

    IEnumerator CallChangeMissileWatchingWithDelay(float delay) {
        yield return new WaitForSeconds(delay);

        m_currentMissileViewDestroyed = false;
        ChangeToMissileWatchingAtOffset(1);
    }

    public void JetExploded(Transform jetTransform) {
        if (m_currentView == View.MISSILE || m_currentJetViewDestroyed) {
            return;
        }

        if (jetTransform == GetTransformOfCurrentJet()) {
            m_currentJetViewDestroyed = true;
            StartCoroutine(CallChangeTeamWatchingWithDelay(m_changeViewIfCurrentJetDestroyedAfterTime));
        }
    }

    public void MissileExploded(Transform missileTransform) {
        if (m_currentView != View.MISSILE || m_currentMissileViewDestroyed) {
            return;
        }

        if (missileTransform == GetTransformOfCurrentView()) {
            m_currentMissileViewDestroyed = true;
            StartCoroutine(CallChangeMissileWatchingWithDelay(m_changeViewIfCurrentMissileDestroyedAfterTime));
        }
    }

    private void ApplyCameraChanges() {
        m_firstPersonCinemachinesArray[m_currentFirstPersonCinemachine].Priority = 0;
        m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].Priority = 0;

        m_currentThirdPersonCinemachine = 1 - m_currentThirdPersonCinemachine;
        m_currentFirstPersonCinemachine = 1 - m_currentFirstPersonCinemachine;

        SetTransformOfCurrentView();
        SetAircraftControllerOfCurrentJet();

        switch (m_currentView) {
            case View.FIRST_PERSON:
                m_firstPersonCinemachinesArray[m_currentFirstPersonCinemachine].Priority = 1;
                m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].Priority = 0;
                break;

            case View.THIRD_PERSON:
                m_firstPersonCinemachinesArray[m_currentFirstPersonCinemachine].Priority = 0;
                m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].Priority = 1;
                break;

            case View.MISSILE:
                m_firstPersonCinemachinesArray[m_currentFirstPersonCinemachine].Priority = 0;
                m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].Priority = 1;
                break;
        }

        UpdateDebugData();
    }

    // Setters --------------------------------------------

    public void SetTheaterComponents(TheaterComponents theaterComponents) {
        m_theaterComponents = theaterComponents;
    }

    private void SetTransformOfCurrentView() {
        switch (m_currentView) {
            case View.FIRST_PERSON:
                m_firstPersonCinemachinesArray[m_currentFirstPersonCinemachine].m_Follow = GetTransformOfCurrentView();
                m_firstPersonCinemachinesArray[m_currentFirstPersonCinemachine].m_LookAt = GetTransformOfCurrentView().GetChild(0);
                break;

            case View.THIRD_PERSON:
                m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].m_Follow = GetTransformOfCurrentView();
                m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].m_LookAt = GetTransformOfCurrentView();
                break;

            case View.MISSILE:
                m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].m_Follow = GetTransformOfCurrentView();
                m_thirdPersonCinemachinesArray[m_currentThirdPersonCinemachine].m_LookAt = GetTransformOfCurrentView();
                break;
        }
    }

    private void SetAircraftControllerOfCurrentJet() {
        if (m_lastJetViewObject != null && m_lastJetViewObject.GetComponent<JetAgent>() != null)
            m_lastJetViewObject.GetComponent<JetAgent>().SetControlledByPlayer(false);

        GameObject jetObject = m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetJetData(m_currentTeamView).GetObject();

        if (jetObject.GetComponent<JetAgent>() != null) {
            m_jetController.SetCurrentAircraftController(null);
            jetObject.GetComponent<JetAgent>().SetControlledByPlayer(true);
        } else {
            m_jetController.SetCurrentAircraftController(
                jetObject.GetComponent<AircraftController>()
            );
        }

        m_lastJetViewObject = jetObject;
    }

    // Getters --------------------------------------------

    private Transform GetTransformOfCurrentJet() {
        JetData jetData = m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetJetData(m_currentTeamView);
        if (jetData == null || jetData.GetObject() == null) {
            return null;
        }

        return jetData.GetObject().transform;
    }

    private Transform GetTransformOfCurrentView() {
        switch (m_currentView) {
            case View.FIRST_PERSON:
                return m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetFirstPersonViewTeam(m_currentTeamView);
            case View.THIRD_PERSON:
                return m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetThirdPersonViewTeam(m_currentTeamView);
            case View.MISSILE:
                MissileData missileData = m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetMissileDataCurrentView();
                if (missileData == null) {
                    return null;
                }

                return missileData.GetMissilePhysics().transform;
            default:
                return null;
        }
    }

    private bool IsCurrentJetViewDestroyed() {
        JetData jetData = m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetJetData(m_currentTeamView);
        if (jetData == null || jetData.GetObject() == null || !jetData.GetObject().activeSelf) {
            return true;
        }

        return false;
    }

    private bool AreAllJetsInSceneViewDestroyed() {
        return m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().AreAllJetsDestroyed();
    }

    private bool HaveCurrentSceneLaunchedMissiles() {
        SceneComponents sceneComponents = m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents();

        return sceneComponents.GetLaunchedMissilesCount() > 0;
    }

    public Scenario GetScenario() {
        return m_scenario;
    }

    public static int GetNumTeams() {
        return System.Enum.GetValues(typeof(Team)).Length;
    }

    public static bool GetRebuildScene() {
        return true;
    }

    public static Resolution GetResolution() {
        return resolution;
    }

    // Debug --------------------------------------------

    private DebugMode m_debugMode;

    private void ConfigureDebugMode() {
        m_debugMode = FindObjectOfType<DebugMode>();
    }

    private void UpdateDebugData() {
        if (m_debugMode == null || !m_debugMode.IsActive()) {
            return;
        }

        m_debugMode.SetTeam(m_currentTeamView);
        m_debugMode.SetScene(m_currentSceneView);
    }

}
