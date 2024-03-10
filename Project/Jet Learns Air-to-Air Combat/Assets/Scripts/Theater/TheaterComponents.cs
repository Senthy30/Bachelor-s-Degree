using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheaterComponents {

    private List<SceneData> sceneArray;

    public TheaterComponents() {
        sceneArray = new List<SceneData>();
    }

    public void AddScene(SceneData scene) {
        sceneArray.Add(scene);
    }

    // Getters ------------------------------------------------
    // Scene ---------------------------------------------------

    public List<SceneData> GetScenes() {
        return sceneArray;
    }

    public SceneData GetScene(int index) {
        return sceneArray[index];
    }
}
