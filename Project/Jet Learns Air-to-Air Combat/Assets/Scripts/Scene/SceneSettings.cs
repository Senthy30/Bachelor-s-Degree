using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneSettings : MonoBehaviour {

    private const float yAircraftCarrier = -20.5f;
    private const float sizePlane = 10f;

    private const string modelObjectName = "Model";
    private const string lowResolutionObjectName = "Low Resolution";
    private const string highResolutionObjectName = "High Resolution";

    private const string thirdPersonViewName = "Third Person View";

    private const string nameJetSpawnPointObject = "Jet Spawn Point";

    private static int nextIdScene = 0; 
    private int idScene;
    [SerializeField]
    private Resolution resolution;
    [SerializeField]
    private Vector2Int sceneSize;
    [SerializeField]
    private Vector2Int sceneCenter;
    [SerializeField]
    private float margin;

    [Header("Enemies Generation")]

    [SerializeField]
    private bool showChunksEnemies;
    [SerializeField]
    private int minDistanceChunksEnemies;
    [SerializeField]
    private Vector2Int numChunksEnemiesSpawn;
    [SerializeField]
    private Material chunkMaterial;

    [Header("Aircraft Carrier Generation")]

    [SerializeField]
    private GameObject aircraftCarrierParentObject;
    [SerializeField]
    private GameObject aircraftCarrierGameObject;

    private List<GameObject> instanceAircraftCarrierGameObject = new List<GameObject>();
    private List<Vector2Int> listOfValidAircraftCarrierCoords = new List<Vector2Int>();

    [Header("Jet Generation")]

    [SerializeField]
    private GameObject jetsParentObject;
    [SerializeField]
    private List <GameObject> teamJetGameObject = new List<GameObject>();

    private List<GameObject> instanceJetGameObject = new List<GameObject>();
    private List<Transform> teamJetSpawnPoint = new List<Transform>();

    [Header("Water Generation")]

    [SerializeField]
    private GameObject waterParentObject;
    [SerializeField]
    private GameObject waterGameObject;
    [SerializeField]
    private Vector2Int waterNumChunks;
    [SerializeField]
    private Material waterMaterial;

    [Header("Missile Generation")]

    [SerializeField]
    private GameObject missileParentObject;
    private List<HeatEmission> heatEmissionArray = new List<HeatEmission>();

    private SceneData sceneData = new SceneData();

    private void Awake() {
        BuildScene();
    }

    public void BuildScene() {
        idScene = nextIdScene++;
        sceneData.ResetData();
        ClearDataForAircraftCarrier();
        ClearDataForJet();

        BuildComponent(Component.WATER, waterParentObject);
        BuildComponent(Component.AIRCRAFT_CARRIER, aircraftCarrierParentObject);
        BuildComponent(Component.JET, jetsParentObject);
        BuildEnemiesChunks();
    }

    private void BuildComponent(Component nameComponent, GameObject parentObject) {
        Transform lowResolutionParent = parentObject.transform.Find(lowResolutionObjectName);
        Transform highResolutionParent = parentObject.transform.Find(highResolutionObjectName);

        Debug.Assert(lowResolutionParent != null, "Low resolution object for" + nameComponent + "is missing!");
        Debug.Assert(highResolutionParent != null, "High resolution object for" + nameComponent + "is missing!");

        while (lowResolutionParent.childCount > 0)
            DestroyImmediate(lowResolutionParent.GetChild(0).gameObject);

        while (highResolutionParent.childCount > 0)
            DestroyImmediate(highResolutionParent.GetChild(0).gameObject);

        if (resolution == Resolution.LOW_RESOLUTION) {
            BuildLowResolutionComponent(nameComponent, lowResolutionParent);
        } else if (resolution == Resolution.HIGH_RESOLUTION) {
            BuildHighResolutionComponent(nameComponent, highResolutionParent);
        }
    }

    private void BuildLowResolutionComponent(Component nameComponent, Transform parentObject) {
        switch (nameComponent) {
            case Component.WATER:
                BuildLowResolutionWater(parentObject); 
                break;

            case Component.JET:
                BuildJet(parentObject); 
                break;

            case Component.AIRCRAFT_CARRIER:
                BuildAircraftCarrier(parentObject);
                break;
        }
    }

    private void BuildHighResolutionComponent(Component nameComponent, Transform parentObject) {
        switch (nameComponent) {
            case Component.WATER:
                BuildHighResolutionWater(parentObject);
                break;

            case Component.JET:
                BuildJet(parentObject);
                break;

            case Component.AIRCRAFT_CARRIER:
                BuildAircraftCarrier(parentObject);
                break;
        }
    }

    // -----------------------------------------------------------------
    // Aircraft Carrier Build

    private void ClearDataForAircraftCarrier() {
        int numTeams = TheaterSettings.GetNumTeams();

        listOfValidAircraftCarrierCoords.Clear();
        for (int x = 0; x < numChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < numChunksEnemiesSpawn.y; y++) {
                listOfValidAircraftCarrierCoords.Add(new Vector2Int(x, y));
            }
        }

        instanceAircraftCarrierGameObject.Clear();
        instanceAircraftCarrierGameObject = new List<GameObject>(new GameObject[numTeams]);
    }

    private void BuildAircraftCarrier(Transform aircraftCarrierParentTransform) {
        foreach (Team team in System.Enum.GetValues(typeof(Team))) {
            Vector3 teamPosition;
            Quaternion teamRotation;
            if (TheaterSettings.GetRebuildScene()) {
                teamPosition = GetNextValidCoordsForAircraftCarrier();
                teamRotation = GetRandomRotation();
            } else {
                teamPosition = TheaterSettings.GetSavedAircraftPositionsByTeam(idScene, team);
                teamRotation = TheaterSettings.GetSavedAircraftRotationByTeam(idScene, team);
            }

            GameObject teamAircraftCarrier = Instantiate(aircraftCarrierGameObject, teamPosition, teamRotation, aircraftCarrierParentTransform);
            teamAircraftCarrier.name = team + " Aircraft Carrier";
            instanceAircraftCarrierGameObject[(int)team] = teamAircraftCarrier;

            GameObject modelObject = teamAircraftCarrier.transform.Find(modelObjectName).gameObject;
            if (resolution == Resolution.LOW_RESOLUTION) {
                modelObject.transform.Find(lowResolutionObjectName).gameObject.SetActive(true);
                modelObject.transform.Find(highResolutionObjectName).gameObject.SetActive(false);
            } else if (resolution == Resolution.HIGH_RESOLUTION) {
                modelObject.transform.Find(lowResolutionObjectName).gameObject.SetActive(false);
                modelObject.transform.Find(highResolutionObjectName).gameObject.SetActive(true);
            }

            teamJetSpawnPoint[(int)team] = teamAircraftCarrier.transform.Find(nameJetSpawnPointObject).transform;

            Debug.Assert(teamJetSpawnPoint[((int)team)] != null, team + " jet spawn point doesn't exists!");
        }
    }

    private Quaternion GetRandomRotation() {
        return Quaternion.Euler(0f, UnityEngine.Random.Range(-179f, 179f), 0f);
    }

    private Vector3 GetNextValidCoordsForAircraftCarrier() {
        int validPositions = listOfValidAircraftCarrierCoords.Count;
        Vector2 length = ((Vector2)(sceneSize) * sizePlane - 2 * margin * Vector2.one) / sizePlane;
        Vector2 chunkSize = new Vector2(length.x / numChunksEnemiesSpawn.x, length.y / numChunksEnemiesSpawn.y);
        Vector3 centerFirstChunk = new Vector3(chunkSize.x - length.x, 0f, chunkSize.y - length.y) * sizePlane / 2f;
        // new Vector3(x * chunkSize.x, 0f, y * chunkSize.y) * sizePlane + centerFirstChunk

        Debug.Assert(validPositions != 0, "No valid positions remained!");
        if (validPositions == 0)
            return Vector3.zero;

        Vector2Int chunkVal = listOfValidAircraftCarrierCoords[UnityEngine.Random.Range(0, validPositions)];
        Vector3 centerChunk = new Vector3(chunkVal.x * chunkSize.x, 0f, chunkVal.y * chunkSize.y) * sizePlane + centerFirstChunk;
        Vector3 offsetInChunk = new Vector3(
            (UnityEngine.Random.Range(0, chunkSize.x) - chunkSize.x / 2) * sizePlane,
            yAircraftCarrier,
            (UnityEngine.Random.Range(0, chunkSize.y) - chunkSize.y / 2) * sizePlane
        );

        int valToDelete = 0;
        for (int i = validPositions - 1; i >= 0; i--) {
            int x = listOfValidAircraftCarrierCoords[i].x;
            int y = listOfValidAircraftCarrierCoords[i].y;

            if (Mathf.Abs(chunkVal.x - x) <= minDistanceChunksEnemies && Mathf.Abs(chunkVal.y - y) <= minDistanceChunksEnemies) {
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

    private void BuildEnemiesChunks() {
        const string nameEnemiesChunks = "Enemies Chunks";
        const float sizePlane = 10f;

        if (transform.Find(nameEnemiesChunks) != null)
            DestroyImmediate(transform.Find(nameEnemiesChunks).gameObject);

        if (!showChunksEnemies)
            return;

        Vector2 length = ((Vector2)(sceneSize) * sizePlane - 2 * margin * Vector2.one) / sizePlane;
        Vector2 chunkSize = new Vector2(length.x / numChunksEnemiesSpawn.x, length.y / numChunksEnemiesSpawn.y);
        Vector3 centerFirstChunk = new Vector3(chunkSize.x - length.x, 0f, chunkSize.y - length.y) * sizePlane / 2f;

        GameObject parentObj = new GameObject(nameEnemiesChunks);

        parentObj.transform.parent = transform;
        parentObj.transform.localPosition = Vector3.zero;

        for (int x = 0; x < numChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < numChunksEnemiesSpawn.y; y++) {
                GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Plane);

                chunk.gameObject.name = "EnemyChunk_" + x + "_" + y;
                chunk.transform.parent = parentObj.transform;
                chunk.transform.localPosition = new Vector3(x * chunkSize.x, 0f, y * chunkSize.y) * sizePlane + centerFirstChunk;
                chunk.transform.localScale = new Vector3(chunkSize.x, 1f, chunkSize.y);
                chunk.GetComponent<Renderer>().sharedMaterial = new Material(chunkMaterial);

                if ((x + y) % 2 == 0)
                    chunk.GetComponent<Renderer>().sharedMaterial.color = Color.black;
                else chunk.GetComponent<Renderer>().sharedMaterial.color = Color.white;
            }
        }
    }

    public Tuple <Vector3, Quaternion> GetAircraftCarrierPositionAndRotationByTeam(Team team) {
        return new Tuple<Vector3, Quaternion>(
            instanceAircraftCarrierGameObject[(int)team].transform.position,
            instanceAircraftCarrierGameObject[(int)team].transform.rotation
        );
    }

    // -----------------------------------------------------------------
    // Jet Build

    private void ClearDataForJet() {
        int numTeams = TheaterSettings.GetNumTeams();

        instanceJetGameObject.Clear();
        instanceJetGameObject = new List<GameObject>(new GameObject[numTeams]);

        teamJetSpawnPoint.Clear();
        teamJetSpawnPoint = new List<Transform>(new Transform[numTeams]);
    }

    private void BuildJet(Transform jetParentTransform) {
        foreach (Team team in System.Enum.GetValues(typeof(Team))) {
            GameObject teamJet = Instantiate(
                teamJetGameObject[(int)team],
                teamJetSpawnPoint[(int)team].position,
                teamJetSpawnPoint[(int)team].rotation,
                jetParentTransform
            );

            GameObject modelObject = teamJet.transform.Find(modelObjectName).gameObject;
            if (resolution == Resolution.LOW_RESOLUTION) {
                modelObject.transform.Find(lowResolutionObjectName).gameObject.SetActive(true);
                modelObject.transform.Find(highResolutionObjectName).gameObject.SetActive(false);
            } else if (resolution == Resolution.HIGH_RESOLUTION) {
                modelObject.transform.Find(lowResolutionObjectName).gameObject.SetActive(false);
                modelObject.transform.Find(highResolutionObjectName).gameObject.SetActive(true);
            }

            heatEmissionArray.Add(new HeatEmission(teamJet.transform, 1));

            AircraftController aircraftController = teamJet.GetComponent<AircraftController>();
            aircraftController.SetSceneMissileParentObject(missileParentObject.transform);
            aircraftController.SetSceneObject(this.gameObject);
            aircraftController.AddMissilesInArray();

            teamJet.name = team + " Jet";
            instanceJetGameObject[(int)team] = teamJet;
        }

        MissilePhysics.SetHeatEmissionArray(heatEmissionArray);

        UpdateSceneData(Component.JET);
    }

    // -----------------------------------------------------------------
    // Water Build

    private void BuildLowResolutionWater(Transform waterParentTransform) {
        GameObject waterChunk = GameObject.CreatePrimitive(PrimitiveType.Plane);

        waterChunk.gameObject.name = "WaterChunk";
        waterChunk.transform.parent = waterParentTransform;
        waterChunk.transform.localPosition = Vector3.zero;
        waterChunk.transform.localScale = new Vector3(sceneSize.x, 0f, sceneSize.y);

        waterChunk.GetComponent<Renderer>().sharedMaterial.color = new Color(66, 123, 245) / 255f;
    }

    private void BuildHighResolutionWater(Transform waterParentTransform) {
        const float sizePlane = 10f;
        Vector2 waterSize = new Vector2(1f * sceneSize.x / waterNumChunks.x, 1f * sceneSize.y / waterNumChunks.y);
        Vector3 centerFirstChunk = new Vector3(waterSize.x - sceneSize.x, 0f, waterSize.y - sceneSize.y) * sizePlane / 2f;

        for (int x = 0; x < waterNumChunks.x; x++) {
            for (int y = 0; y < waterNumChunks.y; y++) {
                GameObject waterChunk = GameObject.CreatePrimitive(PrimitiveType.Plane);

                waterChunk.gameObject.name = "WaterChunk_" + x + "_" + y;
                waterChunk.transform.parent = waterParentTransform;
                waterChunk.transform.localPosition = new Vector3(x * waterSize.x * sizePlane, 0f, y * waterSize.y * sizePlane) + centerFirstChunk;
                waterChunk.transform.localScale = new Vector3(waterSize.x, 1f, waterSize.y);

                Material material = new Material(waterMaterial);
                material.SetVector("_Offset_World", new Vector2(waterNumChunks.x - x, waterNumChunks.y - y));

                waterChunk.GetComponent<Renderer>().material = material;
            }
        }
    }

    // -----------------------------------------------------------------
    // Update Scene Data

    private void UpdateSceneData(Component component) {
        switch (component) {
            case Component.WATER:
                UpdateSceneDataWater();
                break;

            case Component.JET:
                UpdateSceneDataJet();
                break;

            case Component.AIRCRAFT_CARRIER:
                UpdateSceneDataAircraftCarrier();
                break;
        }
    }

    private void UpdateSceneDataWater() {
        
    }

    private void UpdateSceneDataJet() {
        foreach (Team team in System.Enum.GetValues(typeof(Team))) {
            sceneData.AddThirdPersonViewTeam(team, instanceJetGameObject[(int)team].transform.Find(thirdPersonViewName));
        }
    }

    private void UpdateSceneDataAircraftCarrier() {

    }

    public Transform GetThirdPersonViewTeam(Team team) {
        return sceneData.GetThirdPersonViewTeam(team);
    }

    // -----------------------------------------------------------------
    // Getters and Setters

    public static void SetNextIdScene(int id) {
        nextIdScene = id;
    }

    public static int GetNextIdScene() {
        return nextIdScene;
    }
}
