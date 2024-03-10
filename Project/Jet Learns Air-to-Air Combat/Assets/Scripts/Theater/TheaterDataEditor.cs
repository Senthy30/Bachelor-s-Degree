#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TheaterData))]
public class TheaterDataEditor : Editor {
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        TheaterData theaterData = (TheaterData)target;
        if (GUILayout.Button("Regenerate")) {
            theaterData.BuildTheater(true);
            Debug.Log("Generated");
        }
    }

}

#endif
