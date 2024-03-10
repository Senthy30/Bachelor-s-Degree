using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoxDataUtils {
    
    public static GameObject GetBoxObjectFromScene(GameObject sceneObject, SceneConfig sceneConfig) {
        return sceneObject.transform.Find(sceneConfig.nameBoxParentObject).gameObject;
    }

    public static void SetLowNHighBoxObjectActive(GameObject boxObject, SceneConfig sceneConfig) {
        GameObject lowResolutionBoxModel = GetLowResolutionBoxObject(boxObject, sceneConfig);
        GameObject highResolutionBoxModel = GetHighResolutionBoxObject(boxObject, sceneConfig);

        lowResolutionBoxModel.SetActive(
            (sceneConfig.resolution == Resolution.LOW_RESOLUTION) ? true : false
        );
        highResolutionBoxModel.SetActive(
            (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) ? true : false
        );
    }

    private static GameObject GetLowResolutionBoxObject(GameObject boxObject, SceneConfig sceneConfig) {
        return boxObject.transform.Find(sceneConfig.lowResolutionObjectName).gameObject;
    }

    private static GameObject GetHighResolutionBoxObject(GameObject boxObject, SceneConfig sceneConfig) {
        return boxObject.transform.Find(sceneConfig.highResolutionObjectName).gameObject;
    }

}
