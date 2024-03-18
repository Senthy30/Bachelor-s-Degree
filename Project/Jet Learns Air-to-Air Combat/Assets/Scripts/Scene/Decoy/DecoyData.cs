using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoyData {

    private HeatEmission m_heatEmission;
    private SceneData m_sceneData;
    private GameObject m_object;

    public DecoyData(GameObject decoyPrefab, Transform missileParentTransform, Vector3 position, SceneData sceneData) {
        m_object = InstantiateDecoyObject(decoyPrefab, missileParentTransform, position);
        m_sceneData = sceneData;

        m_object.GetComponent<DecoyPhysics>().SetDecoyData(this);
    }

    private GameObject InstantiateDecoyObject(GameObject decoyPrefab, Transform missileParentTransform, Vector3 position) {
        return Object.Instantiate(
            decoyPrefab, position, Quaternion.identity, missileParentTransform
        );
    }

    public void OnDestroy() {
        m_sceneData.DestroyDecoy(this);
    }

    public GameObject GetObject() {
        return m_object;
    }

    public HeatEmission GetHeatEmission() {
        return m_heatEmission;
    }

    public void SetHeatEmission(HeatEmission heatEmission) {
        m_heatEmission = heatEmission;
    }


}
