using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetCollision {

    private static SceneConfig sceneConfig;

    private int indexForPhysicsCalculationArray;
    private GameObject m_object;

    public JetCollision(GameObject jet) {
        m_object = jet;
    }

    public void OnCollisionEnter(Collision collision) {     
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

        Debug.Log("Explode");
        Object.FindObjectOfType<TheaterPhysicsCalculation>().MarkAircraftPyhsicsForSkipping(indexForPhysicsCalculationArray, 1);
        if (m_object != null) {
            Object.Destroy(m_object);
        }
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
