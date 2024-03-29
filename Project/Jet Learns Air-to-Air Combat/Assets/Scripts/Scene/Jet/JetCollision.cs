using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetCollision {

    private static SceneConfig sceneConfig;

    private int indexForPhysicsCalculationArray;
    private Team m_team;
    private SceneData m_sceneData;
    private GameObject m_object;

    public JetCollision(Team team, GameObject jet, SceneData sceneData) {
        m_team = team;
        m_object = jet;
        m_sceneData = sceneData;
    }

    public void OnCollisionEnter(Collision collision) {
        if (!m_object.activeSelf)
            return;

        bool wheelTouchedRunway = true;

        for (int i = 0; i < collision.contacts.Length; i++) {
            int thisColliderLayer = collision.contacts[i].thisCollider.gameObject.layer;
            int otherColliderLayer = collision.contacts[i].otherCollider.gameObject.layer;

            if (otherColliderLayer == LayerMask.NameToLayer(sceneConfig.DECOY_LAYER_NAME)) {
                continue;
            }

            if (!CheckWheelTouchedRunway(thisColliderLayer, otherColliderLayer)) {
                wheelTouchedRunway = false;
            }
        }

        if (!wheelTouchedRunway) {
            Explode();
        }
    }

    public void SetPhysicsCalculationArrayIndex(int index) {
        indexForPhysicsCalculationArray = index;
    }

    private void Explode() {
        if (s_debugIndistractable) {
            return;
        }

        Object.FindObjectOfType<TheaterPhysicsCalculation>().MarkAircraftPyhsicsForSkipping(indexForPhysicsCalculationArray, 1);
        Object.FindObjectOfType<TheaterData>().JetExploded(m_object.transform);

        m_object.GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_object.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        m_object.GetComponent<AircraftController>().ReconfigToInitialState();
        m_object.gameObject.SetActive(false);
        m_sceneData.JetExploded(m_team);
    }

    private bool CheckWheelTouchedRunway(int thisColliderLayer, int otherColliderLayer) {
        return thisColliderLayer == LayerMask.NameToLayer(sceneConfig.WHEEL_LAYER_NAME) 
            && otherColliderLayer == LayerMask.NameToLayer(sceneConfig.AIRCRAFT_RUNWAY_LAYER_NAME);
    }

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }

    // DEBUG MODE

    private static bool s_debugIndistractable = false;

    public static void SetIndistructable(bool debugIndistractable) {
        s_debugIndistractable = debugIndistractable;
    }

}
