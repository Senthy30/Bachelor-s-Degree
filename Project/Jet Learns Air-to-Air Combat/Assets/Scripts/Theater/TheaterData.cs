using Cinemachine;
using UnityEngine;

public class TheaterData : MonoBehaviour {

    private const string PATH_SAVED_POSITION_ROTATION = "Assets/Data/Aircraft Carrier/theater_data.json";

    private static Resolution resolution;
    private static TheaterAircraftsCarrierData theaterAircraftsCarrierData;

    [SerializeField] private Resolution m_resolution;
    [SerializeField] private bool m_rebuildScene = false;
    [SerializeField] private Vector3Int m_numScenes;
    private int m_totalScenes;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera m_firstPersonCinemachine = null;
    [SerializeField] private CinemachineFreeLook m_thirdPersonCinemachine = null;

    [Header("Scene")]
    [SerializeField] private JetController m_jetController;
    [SerializeField] private GameObject m_scenePrefab;

    private int m_currentSceneView;
    private Team m_currentTeamView;
    private View m_currentView;
    private TheaterBuilder m_theaterBuilder;
    private TheaterComponents m_theaterComponents;
    private TheaterPhysicsCalculation m_theaterPhysicsCalculation;

    void Awake() {
        BuildTheater(false);
    }

    public void BuildTheater(bool editorMode) {
        DefaultTheaterConfiguration(editorMode);

        m_theaterBuilder = new TheaterBuilder(m_numScenes, gameObject, m_scenePrefab);

        ApplyCameraChanges();
        m_theaterPhysicsCalculation.CallAfterTheaterBuilt();
    }

    private void DefaultTheaterConfiguration(bool editorMode) {
        TheaterBuilder.SetEditorMode(editorMode);
        SceneData.SetNextIdScene(0);

        resolution = m_resolution;
        m_totalScenes = m_numScenes.x * m_numScenes.y * m_numScenes.z;
        m_currentSceneView = 0;
        m_currentTeamView = (Team)0;
        m_currentView = View.THIRD_PERSON;
        m_theaterPhysicsCalculation = gameObject.GetComponent<TheaterPhysicsCalculation>();
    }

    public void ChangeViewMode() {
        if (m_currentView == View.FIRST_PERSON) {
            m_currentView = View.THIRD_PERSON;
        } else if (m_currentView == View.THIRD_PERSON) {
            m_currentView = View.FIRST_PERSON;
        } 

        ApplyCameraChanges();
    }

    public void ChangeSceneWatching() {
        m_currentSceneView = (m_currentSceneView + 1) % m_totalScenes;
        ApplyCameraChanges();
    }

    public void ChangeTeamWatching() {
        m_currentTeamView = (Team)(((int)m_currentTeamView + 1) % GetNumTeams());
        ApplyCameraChanges();
    }

    private void ApplyCameraChanges() {
        SetTransformOfCurrentView();
        SetAircraftControllerOfCurrentJet();

        if (m_currentView == View.FIRST_PERSON) {
            m_firstPersonCinemachine.gameObject.SetActive(true);
            m_thirdPersonCinemachine.gameObject.SetActive(false);
        } else if (m_currentView == View.THIRD_PERSON) {
            m_firstPersonCinemachine.gameObject.SetActive(false);
            m_thirdPersonCinemachine.gameObject.SetActive(true);
        }
    }

    // Setters --------------------------------------------

    public void SetTheaterComponents(TheaterComponents theaterComponents) {
        m_theaterComponents = theaterComponents;
    }

    private void SetTransformOfCurrentView() {
        switch (m_currentView) {
            case View.THIRD_PERSON:
                m_thirdPersonCinemachine.m_Follow = GetTransformOfCurrentView();
                m_thirdPersonCinemachine.m_LookAt = GetTransformOfCurrentView();
                break;
            case View.FIRST_PERSON:
                m_firstPersonCinemachine.m_Follow = GetTransformOfCurrentView();
                m_firstPersonCinemachine.m_LookAt = GetTransformOfCurrentView().GetChild(0);
                break;
        }
    }

    private void SetAircraftControllerOfCurrentJet() {
        m_jetController.SetCurrentAircraftController(
            m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetJetData(m_currentTeamView).GetObject().GetComponent<AircraftController>()
        );
    }

    // Getters --------------------------------------------

    private Transform GetTransformOfCurrentView() {
        switch (m_currentView) {
            case View.THIRD_PERSON:
                return m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetThirdPersonViewTeam(m_currentTeamView);
            case View.FIRST_PERSON:
                return m_theaterComponents.GetScene(m_currentSceneView).GetSceneComponents().GetFirstPersonViewTeam(m_currentTeamView);
            default:
                return null;
        }
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
}
