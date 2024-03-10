using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetData : IJetData {

    private static SceneConfig sceneConfig;

    private Team m_team;
    private GameObject m_object;

    public JetData(Team team, Transform jetSpawnTransform, Transform jetsParentTransform, GameObject sceneObject) {
        m_team = team;
        m_object = JetDataUtils.InstantiateJetObject(team, jetSpawnTransform, jetsParentTransform, sceneConfig);

        BuildConfigJet(sceneObject);
    }

    public Team GetTeam() {
        return m_team;
    }

    public GameObject GetObject() {
        return m_object;
    }

    private void BuildConfigJet(GameObject sceneObject) {
        JetDataUtils.SetLowNHighJetObjectActive(m_object, sceneConfig);
        JetDataUtils.SetLowNHighMissileStorageActive(m_object, sceneConfig);
        JetDataUtils.SetSceneDataForJet(m_object, sceneConfig, sceneObject);

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

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }
}
