using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class JetDataUtils {

    public static GameObject InstantiateJetObject(
        Team team, Transform jetSpawnTransform, Transform jetParentTransform, SceneConfig sceneConfig
    ) {
        GameObject jetPrefab = sceneConfig.teamJetGameObject[0];
        if ((int)team < sceneConfig.teamJetGameObject.Count)
            jetPrefab = sceneConfig.teamJetGameObject[(int)team];

        return GameObject.Instantiate(
            jetPrefab,
            jetSpawnTransform.position,
            jetSpawnTransform.rotation,
            jetParentTransform
        );
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
        Transform missileStorageParentTransform = GetMissileStorageParentObject(jetObject, sceneConfig).transform;
        int countChilds = missileStorageParentTransform.childCount;

        for (int child = 0; child < countChilds; child++) {
            Transform missileParentTransform = missileStorageParentTransform.GetChild(child);

            for (int i = 0; i < missileParentTransform.childCount; i++) {
                GameObject missile = missileParentTransform.GetChild(i).gameObject;

                missile.transform.Find(sceneConfig.lowResolutionObjectName).gameObject.SetActive(
                    (sceneConfig.resolution == Resolution.LOW_RESOLUTION) ? true : false
                );
                missile.transform.Find(sceneConfig.highResolutionObjectName).gameObject.SetActive(
                    (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) ? true : false
                );
            }
        }
    }

    public static void SetSceneDataForJet(Team team, JetData jetData, GameObject jetObject, SceneConfig sceneConfig, GameObject sceneObject) {
        GameObject missileParentObject = GetMissileStorageParentObject(jetObject, sceneConfig);
        AircraftController aircraftController = jetObject.GetComponent<AircraftController>();

        aircraftController.SetTeam(team);
        aircraftController.SetJetData(jetData);
        aircraftController.SetSceneMissileParentObject(missileParentObject.transform);
        aircraftController.SetSceneObject(sceneObject.GetComponent<SceneData>());
        aircraftController.AddMissilesInArray();
    }

    public static void AddMissilesDataInArray(GameObject jetObject, SceneConfig sceneConfig, List<MissileData> missilesData) {
        Transform missileStorageParentTransform = GetMissileStorageParentObject(jetObject, sceneConfig).transform;
        int countChilds = missileStorageParentTransform.childCount;

        for (int child = 0; child < countChilds; child++) {
            Transform missileParentTransform = missileStorageParentTransform.GetChild(child);

            for (int i = 0; i < missileParentTransform.childCount; i++) {
                GameObject missile = missileParentTransform.GetChild(i).gameObject;
                MissilePhysics missilePhysics = missile.GetComponent<MissilePhysics>();

                missilePhysics.CreateMissileData();
                missilesData.Add(missilePhysics.GetMissileData());
            }
        }
    }

    public static void BuildMissileStorage(GameObject jetObject, SceneConfig sceneConfig) {
        Transform missileStorageParentTransform = GetMissileStorageParentObject(jetObject, sceneConfig).transform;
        int countChilds = missileStorageParentTransform.childCount;

        for (int child = 0; child < sceneConfig.numMissilesPerJet; child++) {
            int indexChild = child % countChilds;
            Transform missileParentTransform = missileStorageParentTransform.GetChild(indexChild);

            while (missileParentTransform.childCount > 0)
                Object.DestroyImmediate(missileParentTransform.GetChild(0).gameObject);

            GameObject missilePrefab = sceneConfig.missileGameObject;
            GameObject missile = Object.Instantiate(
                missilePrefab, missileParentTransform.position, missileParentTransform.rotation, missileParentTransform
            );
        }
    }

    public static GameObject GetMissileStorageParentObject(GameObject jetObject, SceneConfig sceneConfig) {
        return jetObject.transform.Find(sceneConfig.nameMissileStorageParentObject).gameObject;
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

}
