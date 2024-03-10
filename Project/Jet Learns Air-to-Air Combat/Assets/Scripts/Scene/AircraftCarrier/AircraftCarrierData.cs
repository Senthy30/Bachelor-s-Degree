using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftCarrierData : IAircraftCarrierData {
    private static SceneConfig sceneConfig;
    private static List<Vector2Int> listOfValidAircraftCarrierCoords = new List<Vector2Int>();

    private int m_idScene;
    private Team m_team;
    private Transform transformJetSpawnPoint;
    private GameObject m_object;

    public AircraftCarrierData(
        int idScene, Team team, Transform aircraftCarrierParentTransform
    ) {
        m_idScene = idScene;
        m_team = team;
        m_object = AircraftCarrierUtils.InstantiateAircraftCarrierObject(
            idScene, team, listOfValidAircraftCarrierCoords, aircraftCarrierParentTransform, sceneConfig
        );

        BuildConfigAircraftCarrier();
    }

    public int GetIdScene() {
        return m_idScene;
    }

    public Team GetTeam() {
        return m_team;
    }

    public Vector3 GetPosition() {
        return m_object.transform.position;
    }

    public Quaternion GetRotation() {
        return m_object.transform.rotation;
    }

    public Transform GetTransformJetSpawnPoint() {
        return transformJetSpawnPoint;
    }

    public GameObject GetObject() {
        return m_object;
    }

    private void BuildConfigAircraftCarrier() {
        AircraftCarrierUtils.SetLowNHighAircraftCarrierObjectActive(m_object, sceneConfig);
        transformJetSpawnPoint = m_object.transform.Find(sceneConfig.nameJetSpawnPointObject).transform;
    }

    public static void SetSceneConfig(SceneConfig config) {
        sceneConfig = config;
    }

    public static void ClearListOfValidAircraftCarrierCoords() {
        listOfValidAircraftCarrierCoords.Clear();
        for (int x = 0; x < sceneConfig.numChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < sceneConfig.numChunksEnemiesSpawn.y; y++) {
                listOfValidAircraftCarrierCoords.Add(new Vector2Int(x, y));
            }
        }
    }
}
