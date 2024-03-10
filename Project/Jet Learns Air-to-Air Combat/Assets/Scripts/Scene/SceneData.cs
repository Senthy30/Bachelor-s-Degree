using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SceneData : MonoBehaviour {

    private static int nextIdScene = 0;
    private int idScene;

    [SerializeField] private SceneConfig sceneConfig;
    private SceneBuilder sceneBuilder = null;
    private SceneController sceneController = null;
    private SceneComponents sceneComponents = null;

    void Awake() {
        BuildScene();
    }

    public void BuildScene() {
        idScene = nextIdScene++;
        SetResolutionSceneConfig();
        SceneComponents.SetSceneConfig(sceneConfig);

        sceneBuilder = new SceneBuilder(idScene, gameObject, sceneConfig);
    }

    // Setters ------------------------------------------------

    public void SetSceneComponents(SceneComponents sceneComponents) {
        this.sceneComponents = sceneComponents;
    }

    public void SetResolutionSceneConfig() {
        if (sceneConfig.resolution != TheaterData.GetResolution())
            sceneConfig.resolution = TheaterData.GetResolution();
    }

    public static void SetNextIdScene(int id) {
        nextIdScene = id;
    }

    // Getters ------------------------------------------------

    public SceneComponents GetSceneComponents() {
        return sceneComponents;
    }

    public SceneConfig GetSceneConfig() {
        return sceneConfig;
    }
}
