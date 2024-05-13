using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxData {
    
    private static SceneConfig sceneConfig;

    private GameObject m_object;

    public BoxData(GameObject sceneObject) {
        m_object = BoxDataUtils.GetBoxObjectFromScene(sceneObject, sceneConfig);
        BuildConfigBox();
    }

    private void BuildConfigBox() {
        BoxDataUtils.SetLowNHighBoxObjectActive(m_object, sceneConfig);

        m_object.transform.localScale = sceneConfig.GetSizePlane() * (Vector3)sceneConfig.sceneSize;
        m_object.transform.localPosition = new Vector3(0f, sceneConfig.sceneSize.y * sceneConfig.GetSizePlane() / 2f - 0.1f, 0f);
    }

    public GameObject GetObject() {
        return m_object;
    }

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }

}
