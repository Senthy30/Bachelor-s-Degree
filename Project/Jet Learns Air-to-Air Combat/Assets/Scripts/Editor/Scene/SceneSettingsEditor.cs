using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneSettings))]
public class SceneSettingsEditor : Editor {
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        SceneSettings sceneSettings = (SceneSettings)target;
        if (GUILayout.Button("Regenerate")) {
            sceneSettings.BuildScene();
            Debug.Log("Generated");
        }
    }

}
