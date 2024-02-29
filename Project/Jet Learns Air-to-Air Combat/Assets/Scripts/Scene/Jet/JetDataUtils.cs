using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class JetDataUtils {

    public static GameObject GetJetPrefabByTeam(SceneConfig sceneConfig, Team team) {
        GameObject jetPrefab = sceneConfig.teamJetGameObject[0];
        if ((int)team < sceneConfig.teamJetGameObject.Count)
            jetPrefab = sceneConfig.teamJetGameObject[(int)team];

        return jetPrefab;
    }

    public static void SetLowNHighJetObjectActive(GameObject jetObject, SceneConfig sceneConfig) {
        GameObject lowResolutionJetModel = GetLowResolutionJetObject(jetObject, sceneConfig);
        GameObject highResolutionJetModel = GetHighResolutionJetObject(jetObject, sceneConfig);

        lowResolutionJetModel.SetActive(
            (sceneConfig.resolution == Resolution.LOW_RESOLUTION) ? true : false
        );
        highResolutionJetModel.SetActive(
            (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) ? true : false
        );
    }

    public static void SetLowNHighMissileStorageActive(GameObject jetObject, SceneConfig sceneConfig) {
        GameObject missileStorageParent = GetMissileStorageParentObject(jetObject, sceneConfig);
        int countChilds = missileStorageParent.transform.childCount;

        for (int child = 0; child < countChilds; child++) {
            GameObject missile = missileStorageParent.transform.GetChild(child).gameObject;

            missile.transform.Find(sceneConfig.lowResolutionObjectName).gameObject.SetActive(
                (sceneConfig.resolution == Resolution.LOW_RESOLUTION) ? true : false
            );
            missile.transform.Find(sceneConfig.highResolutionObjectName).gameObject.SetActive(
                (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) ? true : false
            );
        }
    }

    public static void SetSceneDataForJet(GameObject jetObject, SceneConfig sceneConfig, GameObject sceneObject) {
        GameObject missileParentObject = GetMissileStorageParentObject(jetObject, sceneConfig);
        AircraftController aircraftController = jetObject.GetComponent<AircraftController>();

        aircraftController.SetSceneMissileParentObject(missileParentObject.transform);
        aircraftController.SetSceneObject(sceneObject);
        aircraftController.AddMissilesInArray();
    }

    // Get methods ---------------------------------------------------------

    private static GameObject GetModelObject(GameObject jetObject, SceneConfig sceneConfig) {
        return jetObject.transform.Find(sceneConfig.modelObjectName).gameObject;
    }

    private static GameObject GetLowResolutionJetObject(GameObject jetObject, SceneConfig sceneConfig) {
        GameObject modelObject = GetModelObject(jetObject, sceneConfig);
        GameObject lowResolutionJetModel = modelObject.transform.Find(sceneConfig.lowResolutionObjectName).gameObject;

        return lowResolutionJetModel;
    }

    private static GameObject GetHighResolutionJetObject(GameObject jetObject, SceneConfig sceneConfig) {
        GameObject modelObject = GetModelObject(jetObject, sceneConfig);
        GameObject highResolutionJetModel = modelObject.transform.Find(sceneConfig.highResolutionObjectName).gameObject;

        return highResolutionJetModel;
    }

    private static GameObject GetMissileStorageParentObject(GameObject jetObject, SceneConfig sceneConfig) {
        return jetObject.transform.Find(sceneConfig.nameMissileStorageParentObject).gameObject;
    }

}
