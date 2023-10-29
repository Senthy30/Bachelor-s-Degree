using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData {

    private List <Transform> thirdPersonViewArray = new List <Transform> ();

    public void ResetData() {
        int numTeams = TheaterSettings.GetNumTeams();

        thirdPersonViewArray.Clear();
        thirdPersonViewArray = new List<Transform>(new Transform[numTeams]);
    }

    public void AddThirdPersonViewTeam(Team team, Transform transform) {
        thirdPersonViewArray[((int)team)] = transform;
    }

    public Transform GetThirdPersonViewTeam(Team team) {
        return thirdPersonViewArray[((int)team)];
    }

}

[Serializable]
public struct SceneAircraftsCarrierData {
    public Vector3[] position;
    public Quaternion[] rotation;

    public SceneAircraftsCarrierData(int numTeams) {
        position = new Vector3[numTeams];
        rotation = new Quaternion[numTeams];
    }

    public void SetPositionAndRotationByTeam(Team team, Vector3 position, Quaternion rotation) {
        this.position[(int)team] = position;
        this.rotation[(int)team] = rotation;
    }

    public Vector3 GetPositionByTeam(Team team) {
        return position[(int)team];
    }

    public Quaternion GetRotationByTeam(Team team) {
        return rotation[(int)team];
    }
}

[Serializable]
public struct TheaterAircraftsCarrierData {
    public SceneAircraftsCarrierData[] sceneAircraftsCarrierData;

    public TheaterAircraftsCarrierData(int numScenes, int numTeams) {
        sceneAircraftsCarrierData = new SceneAircraftsCarrierData[numScenes];
        for (int i = 0; i < numScenes; i++)
            sceneAircraftsCarrierData[i] = new SceneAircraftsCarrierData(numTeams);
    }

    public SceneAircraftsCarrierData GetSceneAircraftsCarrierData(int idScene) {
        return sceneAircraftsCarrierData[idScene];
    }
}