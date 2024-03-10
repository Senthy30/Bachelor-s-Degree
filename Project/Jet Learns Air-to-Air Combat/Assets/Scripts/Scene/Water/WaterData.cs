using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterData {

    private static SceneConfig sceneConfig;

    private GameObject m_object;

    public WaterData(GameObject sceneObject) {
        m_object = WaterDataUtils.GetWaterObjectFromScene(sceneObject, sceneConfig);
        WaterDataUtils.InstantiateWater(m_object.transform, sceneConfig);
    }

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }

}
