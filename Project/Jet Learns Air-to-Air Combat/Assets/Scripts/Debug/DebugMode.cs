using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugMode : MonoBehaviour {

    [SerializeField] private bool m_active = false;

    private int m_updateDisplayFrequency = 60;
    private float m_updateDisplayCalculatedTime = 0.0f;
    private float m_updateDisplayTimer = 0.0f;

    [Header("Player")]

    [SerializeField] private bool m_displayTeam;
    [SerializeField] private bool m_displayScene;

    private Team m_team;
    private int m_scene;

    [SerializeField] private TextMeshProUGUI m_teamText;
    [SerializeField] private TextMeshProUGUI m_sceneText;

    [Header("Profiler")]

    [SerializeField] private bool m_displayMiniProfiler = false;
    [SerializeField] private bool m_displayRawFPS = false;

    [SerializeField] private GameObject m_miniProfiler;
    [SerializeField] private GameObject m_rawFPSText;

    [Header("Physics")]

    [SerializeField] private bool m_displayAverageTimeJetPhysics = false;
    [SerializeField] private bool m_displayTimeJetPhysics = false;

    private float m_averageTimeJetPhysicsValue = 0.0f;
    private float m_timeJetPhysicsValue = 0.0f;

    [SerializeField] private TextMeshProUGUI m_averageTimeJetPhysicsText;
    [SerializeField] private TextMeshProUGUI m_timeJetPhysicsText;

    [Header("GPU")]

    [SerializeField] private bool m_displayAverageTimeGPU = false;
    [SerializeField] private bool m_displayLastTimeGPU = false;

    private float m_averageTimeGPUValue = 0.0f;
    private float m_lastTimeGPUValue = 0.0f;

    [SerializeField] private TextMeshProUGUI m_averageTimeGPUText;
    [SerializeField] private TextMeshProUGUI m_lastTimeCPUText;

    [Header("Jet")]

    [SerializeField] private bool m_indistructable = true;

    // Methods ----------------------------------------------------------------

    private void Awake() {
        ConfigDebugData();
    }

    private void Update() {
        UpdateData();
    }

    public void UpdateData() {
        if (!m_active) { return; }

        m_updateDisplayTimer += Time.deltaTime;
        if (m_updateDisplayTimer < m_updateDisplayCalculatedTime) {
            return;
        }
        m_updateDisplayTimer = 0.0f;

        UpdatePlayerData();
        UpdateProfilerData();
        UpdatePhysicsData();
        UpdateGPUData();
        UpdateJetData();
    }

    private void UpdatePlayerData() {
        if (!m_active) { return; }

        if (m_teamText != null && m_displayTeam) {
            m_teamText.text = "Team: " + m_team.ToString();
        }
        if (m_sceneText != null && m_displayScene) {
            m_sceneText.text = "Scene: " + m_scene.ToString();
        }
    }

    private void UpdateProfilerData() {
        if (!m_active) { return; }

        // pass
    }

    private void UpdatePhysicsData() {
        if (!m_active) { return; };

        if (m_averageTimeJetPhysicsText != null && m_displayAverageTimeJetPhysics) {
            m_averageTimeJetPhysicsText.text = "Av.Time Jet Phy.: " + m_averageTimeJetPhysicsValue.ToString();
        }
        if (m_timeJetPhysicsText != null && m_displayTimeJetPhysics) {
            m_timeJetPhysicsText.text = "Time Jet Phy.: " + m_timeJetPhysicsValue.ToString();
        }
    }

    private void UpdateGPUData() {
        if (!m_active) { return; }

        if (m_averageTimeGPUText != null && m_displayAverageTimeGPU) {
            m_averageTimeGPUText.text = "Av.Time  GPU: " + m_averageTimeGPUValue.ToString();
        }
        if (m_lastTimeCPUText != null && m_displayLastTimeGPU) {
            m_lastTimeCPUText.text = "Time GPU: " + m_lastTimeGPUValue.ToString();
        }
    }

    private void UpdateJetData() {
        if (!m_active) { return; };

        JetCollision.SetIndistructable(m_indistructable);
    }

    // Getters --------------------------------------------------------------

    public bool IsActive() {
        return m_active;
    }

    // Setters --------------------------------------------------------------

    public void SetActive(bool active) {
        m_active = active;
    }

    public void SetTeam(Team team) {
        m_team = team;
    }

    public void SetScene(int scene) {
        m_scene = scene;
    }

    public void SetAverageTimeJetPhysics(float time) {
        m_averageTimeJetPhysicsValue = time;
    }

    public void SetTimeJetPhysics(float time) {
        m_timeJetPhysicsValue = time;
    }

    public void SetAverageTimeGPU(float time) {
        m_averageTimeGPUValue = time;
    }

    public void SetLastTimeGPU(float time) {
        m_lastTimeGPUValue = time;
    }

    private void ConfigDebugData() {
        m_updateDisplayCalculatedTime = 1.0f / m_updateDisplayFrequency;

        SetActiveDebugObjects();

        JetCollision.SetIndistructable(m_indistructable);
    }

    private void SetActiveDebugObjects() {
        if (m_teamText != null) {
            m_teamText.gameObject.SetActive((m_active) ? m_displayTeam : false);
        }
        if (m_sceneText != null) {
            m_sceneText.gameObject.SetActive((m_active) ? m_displayScene : false);
        }

        if (m_miniProfiler != null) {
            m_miniProfiler.SetActive((m_active) ? m_displayMiniProfiler : false);
        }
        if (m_rawFPSText != null) {
            m_rawFPSText.SetActive((m_active) ? m_displayRawFPS : false);
        }

        if (m_averageTimeGPUText != null) {
            m_averageTimeGPUText.gameObject.SetActive((m_active) ? m_displayAverageTimeGPU : false);
        }
        if (m_lastTimeCPUText != null) {
            m_lastTimeCPUText.gameObject.SetActive((m_active) ? m_displayLastTimeGPU : false);
        }

        if (m_averageTimeJetPhysicsText != null) {
            m_averageTimeJetPhysicsText.gameObject.SetActive((m_active) ? m_displayAverageTimeJetPhysics : false);
        }
        if (m_timeJetPhysicsText != null) {
            m_timeJetPhysicsText.gameObject.SetActive((m_active) ? m_displayTimeJetPhysics : false);
        }
    }

    private void OnValidate() {
        m_updateDisplayTimer = m_updateDisplayCalculatedTime;
        SetActiveDebugObjects();
        UpdateData();
    }
}
