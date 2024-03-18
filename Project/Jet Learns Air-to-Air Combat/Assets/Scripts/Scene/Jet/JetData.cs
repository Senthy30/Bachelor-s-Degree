using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetData : IJetData {

    private static SceneConfig sceneConfig;

    private int m_numDecoys = 0;
    private Team m_team;
    private AircraftPhysics m_aircraftPhysics;
    private JetCollision m_jetCollision;
    private Transform m_firstPersonViewTransform;
    private Transform m_thirdPersonViewTransform;
    private Transform m_decoySpawnPointTransform;
    private GameObject m_object;

    public JetData(Team team, Transform jetSpawnTransform, Transform jetsParentTransform, GameObject sceneObject) {
        m_team = team;
        m_object = JetDataUtils.InstantiateJetObject(team, jetSpawnTransform, jetsParentTransform, sceneConfig);

        m_aircraftPhysics = m_object.GetComponent<AircraftPhysics>();
        Debug.Assert(m_aircraftPhysics != null, "AircraftPhysics not found in JetData");

        m_jetCollision = new JetCollision(m_object);
        m_aircraftPhysics.AddFuncInOnCollision(m_jetCollision.OnCollisionEnter);

        BuildConfigJet(sceneObject);
    }

    public Team GetTeam() {
        return m_team;
    }

    public Transform GetFirstPersonViewTransform() {
        return m_firstPersonViewTransform;
    }

    public Transform GetThirdPersonViewTransform() {
        return m_thirdPersonViewTransform;
    }

    public Transform GetDecoySpawnPointTransform() {
        return m_decoySpawnPointTransform;
    }

    public GameObject GetObject() {
        return m_object;
    }

    public int GetNumDecoys() {
        return m_numDecoys;
    }

    public void DecrementNumDecoys() {
        m_numDecoys--;
    }

    public void SetNumDecoys(int numDecoys) {
        m_numDecoys = numDecoys;
    }

    public GameObject GetColliderParentObject() {
        return m_object.transform.Find(sceneConfig.collisionParentObjectName).gameObject;
    }

    private void BuildConfigJet(GameObject sceneObject) {
        JetDataUtils.SetLowNHighJetObjectActive(m_object, sceneConfig);
        JetDataUtils.SetLowNHighMissileStorageActive(m_object, sceneConfig);
        JetDataUtils.SetSceneDataForJet(m_team, m_object, sceneConfig, sceneObject);

        m_firstPersonViewTransform = m_object.transform.Find(sceneConfig.firstPersonViewName);
        m_thirdPersonViewTransform = m_object.transform.Find(sceneConfig.thirdPersonViewName);
        m_decoySpawnPointTransform = m_object.transform.Find(sceneConfig.decoySpawnPointName);

        m_object.name = m_team + " Jet";
    }

    public void SetMissilePhysicsHeatEmission(List<HeatEmission> heatEmissionsArray) {
        GameObject missileStorageParent = JetDataUtils.GetMissileStorageParentObject(m_object, sceneConfig);
        int countChilds = missileStorageParent.transform.childCount;

        for (int child = 0; child < countChilds; child++) {
            GameObject missile = missileStorageParent.transform.GetChild(child).gameObject;
            MissilePhysics missilePhysics = missile.GetComponent<MissilePhysics>();

            missilePhysics.SetHeatEmissionArray(heatEmissionsArray);
        }
    }

    public void TriggerMissilesFindTarget() {
        GameObject missileStorageParent = JetDataUtils.GetMissileStorageParentObject(m_object, sceneConfig);
        int countChilds = missileStorageParent.transform.childCount;

        for (int child = 0; child < countChilds; child++) {
            GameObject missile = missileStorageParent.transform.GetChild(child).gameObject;
            MissilePhysics missilePhysics = missile.GetComponent<MissilePhysics>();

            if (missilePhysics.GetMissileLaunched()) {
                missilePhysics.TriggerFindTarget();
            }
        }
    }

    public void SetPhysicsCalculationArrayIndex(int index) {
        m_jetCollision.SetPhysicsCalculationArrayIndex(index);
    }

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }
}
