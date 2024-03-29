using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AircraftCarrierUtils {

    public static GameObject InstantiateAircraftCarrierObject(
        int idScene, Team team, List<Vector2Int> listOfValidAircraftCarrierCoords, 
        Transform aircraftCarrierParentTransform, SceneConfig sceneConfig
    ) {
        Vector3 teamPosition = GetAircraftCarrierPosition(idScene, team, listOfValidAircraftCarrierCoords, sceneConfig);
        Quaternion teamRotation = GetAircraftCarrierRotation(idScene, team);

        GameObject aircraftCarrierObject = Object.Instantiate(
            sceneConfig.aircraftCarrierGameObject, teamPosition, teamRotation, aircraftCarrierParentTransform
        );
        aircraftCarrierObject.transform.localPosition = teamPosition;
        aircraftCarrierObject.name = team + " Aircraft Carrier";

        return aircraftCarrierObject;
    }

    public static void SetLowNHighAircraftCarrierObjectActive(GameObject aircraftCarrierObject, SceneConfig sceneConfig) {
        GameObject modelObject = GetModelObject(aircraftCarrierObject, sceneConfig);
        GameObject lowResolutionAircraftCarrierObject = GetLowResolutionAircraftCarrierObject(modelObject, sceneConfig);
        GameObject highResolutionAircraftCarrierObject = GetHighResolutionAircraftCarrierObject(modelObject, sceneConfig);

        lowResolutionAircraftCarrierObject.SetActive(
            (sceneConfig.resolution == Resolution.LOW_RESOLUTION) ? true : false
        );
        highResolutionAircraftCarrierObject.SetActive(
            (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) ? true : false
        );
    }

    // Get methods ---------------------------------------------------------

    private static GameObject GetModelObject(GameObject aircraftCarrierObject, SceneConfig sceneConfig) {
        return aircraftCarrierObject.transform.Find(sceneConfig.modelObjectName).gameObject;
    }

    private static GameObject GetLowResolutionAircraftCarrierObject(GameObject aircraftCarrierObject, SceneConfig sceneConfig) {
        return aircraftCarrierObject.transform.Find(sceneConfig.lowResolutionObjectName).gameObject;
    }

    private static GameObject GetHighResolutionAircraftCarrierObject(GameObject aircraftCarrierObject, SceneConfig sceneConfig) {
        return aircraftCarrierObject.transform.Find(sceneConfig.highResolutionObjectName).gameObject;
    }

    private static Quaternion GetRandomRotation() {
        return Quaternion.Euler(0f, UnityEngine.Random.Range(-179f, 179f), 0f);
    }

    public static Vector3 GetAircraftCarrierPosition(
        int idScene, Team team, List<Vector2Int> listOfValidAircraftCarrierCoords, SceneConfig sceneConfig
    ) {
        if (TheaterData.GetRebuildScene()) {
            return GetNextValidCoordsForAircraftCarrier(listOfValidAircraftCarrierCoords, sceneConfig);
        } else {
            return Vector3.zero; // TheaterSettings.GetSavedAircraftPositionsByTeam(idScene, team);
        }
    }

    public static Quaternion GetAircraftCarrierRotation(int idScene, Team team) {
        if (TheaterData.GetRebuildScene()) {
            return GetRandomRotation();
        } else {
            return Quaternion.identity; // TheaterSettings.GetSavedAircraftRotationByTeam(idScene, team);
        }
    }

    private static Vector3 GetNextValidCoordsForAircraftCarrier(
        List<Vector2Int> listOfValidAircraftCarrierCoords, SceneConfig sceneConfig
    ) {
        int validPositions = listOfValidAircraftCarrierCoords.Count;
        float sizePlane = sceneConfig.GetSizePlane();

        Vector2Int sceneLW = new Vector2Int(sceneConfig.sceneSize.x, sceneConfig.sceneSize.z);
        Vector2 length = ((Vector2)(sceneLW) * sizePlane - 2 * sceneConfig.margin * Vector2.one) / sizePlane;
        Vector2 chunkSize = new Vector2(
            length.x / sceneConfig.numChunksEnemiesSpawn.x,
            length.y / sceneConfig.numChunksEnemiesSpawn.y
        );
        Vector3 centerFirstChunk = new Vector3(
            chunkSize.x - length.x,
            0f,
            chunkSize.y - length.y
        ) * sizePlane / 2f;

        Debug.Assert(validPositions != 0, "No valid positions remained!");
        if (validPositions == 0)
            return Vector3.zero;

        Vector2Int chunkVal = listOfValidAircraftCarrierCoords[UnityEngine.Random.Range(0, validPositions)];
        Vector3 centerChunk = new Vector3(chunkVal.x * chunkSize.x, 0f, chunkVal.y * chunkSize.y) * sizePlane + centerFirstChunk;
        Vector3 offsetInChunk = new Vector3(
            (UnityEngine.Random.Range(0, chunkSize.x) - chunkSize.x / 2) * sizePlane,
            sceneConfig.yAircraftCarrier,
            (UnityEngine.Random.Range(0, chunkSize.y) - chunkSize.y / 2) * sizePlane
        );

        int valToDelete = 0;
        for (int i = validPositions - 1; i >= 0; i--) {
            int x = listOfValidAircraftCarrierCoords[i].x;
            int y = listOfValidAircraftCarrierCoords[i].y;

            if (Mathf.Abs(chunkVal.x - x) <= sceneConfig.minDistanceChunksEnemies && Mathf.Abs(chunkVal.y - y) <= sceneConfig.minDistanceChunksEnemies) {
                Vector2Int temp = listOfValidAircraftCarrierCoords[i];
                listOfValidAircraftCarrierCoords[i] = listOfValidAircraftCarrierCoords[validPositions - valToDelete - 1];
                listOfValidAircraftCarrierCoords[validPositions - valToDelete - 1] = temp;
                ++valToDelete;
            }
        }

        for (int i = 1; i <= valToDelete; i++)
            listOfValidAircraftCarrierCoords.RemoveAt(validPositions - i);

        return centerChunk + offsetInChunk;
    }

}
