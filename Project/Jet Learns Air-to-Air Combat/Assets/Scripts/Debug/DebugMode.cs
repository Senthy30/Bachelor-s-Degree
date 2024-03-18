using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugMode : MonoBehaviour {

    private static bool active = false;

    [SerializeField] private bool m_active = false;

    private int m_updateDisplayFrequency = 60;
    private float m_updateDisplayCalculatedTime = 0.0f;
    private float m_updateDisplayTimer = 0.0f;

    [Header("Profiler")]

    [SerializeField] private bool m_displayMiniProfiler = false;
    [SerializeField] private bool m_displayRawFPS = false;

    [SerializeField] private GameObject m_miniProfiler;
    [SerializeField] private GameObject m_rawFPSText;

    [Header("Physics")]

    [SerializeField] private bool m_displayAverageTimeJetPhysics = false;
    [SerializeField] private bool m_displayTimeJetPhysics = false;

    [SerializeField] private TextMeshProUGUI m_averageTimeJetPhysicsText;
    [SerializeField] private TextMeshProUGUI m_timeJetPhysicsText;

    [Header("GPU")]

    [SerializeField] private bool m_displayAverageTimeGPU = false;
    [SerializeField] private bool m_displayLastTimeGPU = false;

    [SerializeField] private TextMeshProUGUI m_averageTimeGPUText;
    [SerializeField] private TextMeshProUGUI m_lastTimeCPUText;

    [Header("Jet")]

    [SerializeField] private bool m_indistructable = true;

    private void Awake() {
        active = m_active;
        if (!m_active) {
            DebugInactiveSettings();
            return;
        }

        DebugActiveSettings();
        Debug.Log("Debug Activated");
    }

    private void LateUpdate() {
        if (!m_active) {
            return;
        }

        if (m_updateDisplayTimer >= m_updateDisplayCalculatedTime) {
            m_updateDisplayTimer = 0.0f;
            return;
        }

        m_updateDisplayTimer += Time.deltaTime;
    }

    private void DebugActiveSettings() {
        m_updateDisplayCalculatedTime = 1.0f / m_updateDisplayFrequency;

        SetActiveDebugObjects();

        JetCollision.SetIndistructable(m_indistructable);
    }

    private void DebugInactiveSettings() {
        SetActiveDebugObjects();
    }

    private void SetActiveDebugObjects() {
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

    // Public Methods

    public void UpdateAverageTimeGPU(float time) {
        if (!m_active || !m_displayAverageTimeGPU || m_updateDisplayTimer < m_updateDisplayCalculatedTime) {
            return;
        }

        m_averageTimeGPUText.text = "Av.Time  GPU: " + time.ToString("F2") + "ms";
    }

    public void UpdateLastTimeGPU(float time) {
        if (!m_active || !m_displayLastTimeGPU || m_updateDisplayTimer < m_updateDisplayCalculatedTime) {
            return;
        }

        m_lastTimeCPUText.text = "Time GPU: " + time.ToString("F2") + "ms";
    }

    public void UpdateAverageTimeJetPhysics(float time) {
        if (!m_active || !m_displayAverageTimeJetPhysics || m_updateDisplayTimer < m_updateDisplayCalculatedTime) {
            return;
        }

        m_averageTimeJetPhysicsText.text = "Av.Time Jet Phy.: " + time.ToString("F2") + "ms";
    }

    public void UpdateTimeJetPhysics(float time) {
        if (!m_active || !m_displayTimeJetPhysics || m_updateDisplayTimer < m_updateDisplayCalculatedTime) {
            return;
        }

        m_timeJetPhysicsText.text = "Time Jet Phy.: " + time.ToString("F2") + "ms";
    }

    private void OnValidate() {
        active = m_active;
        if (!m_active) {
            DebugInactiveSettings();
            return;
        }

        DebugActiveSettings();
    }

    public static bool IsActive() {
        return active;
    }
}
