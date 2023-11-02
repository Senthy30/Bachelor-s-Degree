using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneSettings : MonoBehaviour {

    private static int nextIdScene = 0; 
    private int idScene;

    [SerializeField]
    private SceneConfig sceneConfig;

    private GameObject boxParentObject;

    private GameObject aircraftCarrierParentObject;
    private List<GameObject> instanceAircraftCarrierGameObject = new List<GameObject>();
    private List<Vector2Int> listOfValidAircraftCarrierCoords = new List<Vector2Int>();

    private GameObject jetsParentObject;
    private List<GameObject> instanceJetGameObject = new List<GameObject>();
    private List<Transform> teamJetSpawnPoint = new List<Transform>();

    private GameObject waterParentObject;

    private GameObject missileParentObject;

    private List<HeatEmission> heatEmissionArray = new List<HeatEmission>();

    private SceneData sceneData = new SceneData();

    private void Awake() {
        BuildScene();
    }

    public void BuildScene() {
        FindParentsObjects();

        idScene = nextIdScene++;
        sceneData.ResetData();
        ClearDataForAircraftCarrier();
        ClearDataForJet();

        BuildSceneBox();
        BuildComponent(Component.WATER, waterParentObject);
        BuildComponent(Component.AIRCRAFT_CARRIER, aircraftCarrierParentObject);
        BuildComponent(Component.JET, jetsParentObject);
        BuildEnemiesChunks();
    }

    private void BuildSceneBox() {
        boxParentObject.transform.localScale = new Vector3(sceneConfig.sceneSize.x, sceneConfig.sceneSize.y, sceneConfig.sceneSize.z) * sceneConfig.GetSizePlane();
        boxParentObject.transform.localPosition = new Vector3(0f, sceneConfig.sceneSize.y * sceneConfig.GetSizePlane() / 2f - 0.1f, 0f);

        Transform lowResolutionParent = boxParentObject.transform.Find(sceneConfig.lowResolutionObjectName);
        Transform highResolutionParent = boxParentObject.transform.Find(sceneConfig.highResolutionObjectName);

        Debug.Assert(lowResolutionParent != null, "Low resolution object for box is missing!");
        Debug.Assert(highResolutionParent != null, "High resolution object for box is missing!");

        if (sceneConfig.resolution == Resolution.LOW_RESOLUTION)
            highResolutionParent.gameObject.SetActive(false);
        else if (sceneConfig.resolution == Resolution.HIGH_RESOLUTION)
            lowResolutionParent.gameObject.SetActive(false);
    }

    private void BuildComponent(Component nameComponent, GameObject parentObject) {
        Transform lowResolutionParent = parentObject.transform.Find(sceneConfig.lowResolutionObjectName);
        Transform highResolutionParent = parentObject.transform.Find(sceneConfig.highResolutionObjectName);

        Debug.Assert(lowResolutionParent != null, "Low resolution object for" + nameComponent + "is missing!");
        Debug.Assert(highResolutionParent != null, "High resolution object for" + nameComponent + "is missing!");

        while (lowResolutionParent.childCount > 0)
            DestroyImmediate(lowResolutionParent.GetChild(0).gameObject);

        while (highResolutionParent.childCount > 0)
            DestroyImmediate(highResolutionParent.GetChild(0).gameObject);

        if (sceneConfig.resolution == Resolution.LOW_RESOLUTION) {
            BuildLowResolutionComponent(nameComponent, lowResolutionParent);
        } else if (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) {
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
        for (int x = 0; x < sceneConfig.numChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < sceneConfig.numChunksEnemiesSpawn.y; y++) {
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

            GameObject teamAircraftCarrier = Instantiate(sceneConfig.aircraftCarrierGameObject, teamPosition, teamRotation, aircraftCarrierParentTransform);
            teamAircraftCarrier.transform.localPosition = teamPosition;
            teamAircraftCarrier.name = team + " Aircraft Carrier";
            instanceAircraftCarrierGameObject[(int)team] = teamAircraftCarrier;

            GameObject modelObject = teamAircraftCarrier.transform.Find(sceneConfig.modelObjectName).gameObject;
            if (sceneConfig.resolution == Resolution.LOW_RESOLUTION) {
                modelObject.transform.Find(sceneConfig.lowResolutionObjectName).gameObject.SetActive(true);
                modelObject.transform.Find(sceneConfig.highResolutionObjectName).gameObject.SetActive(false);
            } else if (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) {
                modelObject.transform.Find(sceneConfig.lowResolutionObjectName).gameObject.SetActive(false);
                modelObject.transform.Find(sceneConfig.highResolutionObjectName).gameObject.SetActive(true);
            }

            teamJetSpawnPoint[(int)team] = teamAircraftCarrier.transform.Find(sceneConfig.nameJetSpawnPointObject).transform;

            Debug.Assert(teamJetSpawnPoint[((int)team)] != null, team + " jet spawn point doesn't exists!");
        }
    }

    private Quaternion GetRandomRotation() {
        return Quaternion.Euler(0f, UnityEngine.Random.Range(-179f, 179f), 0f);
    }

    private Vector3 GetNextValidCoordsForAircraftCarrier() {
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
        // new Vector3(x * chunkSize.x, 0f, y * chunkSize.y) * sizePlane + centerFirstChunk

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

    private void BuildEnemiesChunks() {
        const string nameEnemiesChunks = "Enemies Chunks";
        const float sizePlane = 10f;

        if (transform.Find(nameEnemiesChunks) != null)
            DestroyImmediate(transform.Find(nameEnemiesChunks).gameObject);

        if (!sceneConfig.showChunksEnemies)
            return;

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

        GameObject parentObj = new GameObject(nameEnemiesChunks);

        parentObj.transform.parent = transform;
        parentObj.transform.localPosition = Vector3.zero;

        for (int x = 0; x < sceneConfig.numChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < sceneConfig.numChunksEnemiesSpawn.y; y++) {
                GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Plane);

                chunk.gameObject.name = "EnemyChunk_" + x + "_" + y;
                chunk.transform.parent = parentObj.transform;
                chunk.transform.localPosition = new Vector3(x * chunkSize.x, 0f, y * chunkSize.y) * sizePlane + centerFirstChunk;
                chunk.transform.localScale = new Vector3(chunkSize.x, 1f, chunkSize.y);
                chunk.GetComponent<Renderer>().sharedMaterial = new Material(sceneConfig.chunkMaterial);

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
            GameObject jetPrefab = sceneConfig.teamJetGameObject[0];
            if ((int)team < sceneConfig.teamJetGameObject.Count)
                jetPrefab = sceneConfig.teamJetGameObject[(int)team];

            GameObject teamJet = Instantiate(
                jetPrefab,
                teamJetSpawnPoint[(int)team].position,
                teamJetSpawnPoint[(int)team].rotation,
                jetParentTransform
            );

            GameObject modelObject = teamJet.transform.Find(sceneConfig.modelObjectName).gameObject;
            if (sceneConfig.resolution == Resolution.LOW_RESOLUTION) {
                modelObject.transform.Find(sceneConfig.lowResolutionObjectName).gameObject.SetActive(true);
                modelObject.transform.Find(sceneConfig.highResolutionObjectName).gameObject.SetActive(false);
            } else if (sceneConfig.resolution == Resolution.HIGH_RESOLUTION) {
                modelObject.transform.Find(sceneConfig.lowResolutionObjectName).gameObject.SetActive(false);
                modelObject.transform.Find(sceneConfig.highResolutionObjectName).gameObject.SetActive(true);
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
        waterChunk.transform.localScale = new Vector3(sceneConfig.sceneSize.x, 0f, sceneConfig.sceneSize.z);

        waterChunk.GetComponent<Renderer>().sharedMaterial = new Material(sceneConfig.waterMaterialLowResolution);
    }

    private void BuildHighResolutionWater(Transform waterParentTransform) {
        const float sizePlane = 10f;
        Vector2 waterSize = new Vector2(1f * sceneConfig.sceneSize.x / sceneConfig.waterNumChunks.x, 1f * sceneConfig.sceneSize.z / sceneConfig.waterNumChunks.y);
        Vector3 centerFirstChunk = new Vector3(waterSize.x - sceneConfig.sceneSize.x, 0f, waterSize.y - sceneConfig.sceneSize.z) * sizePlane / 2f;

        for (int x = 0; x < sceneConfig.waterNumChunks.x; x++) {
            for (int y = 0; y < sceneConfig.waterNumChunks.y; y++) {
                GameObject waterChunk = GameObject.CreatePrimitive(PrimitiveType.Plane);

                waterChunk.gameObject.name = "WaterChunk_" + x + "_" + y;
                waterChunk.transform.parent = waterParentTransform;
                waterChunk.transform.localPosition = new Vector3(x * waterSize.x * sizePlane, 0f, y * waterSize.y * sizePlane) + centerFirstChunk;
                waterChunk.transform.localScale = new Vector3(waterSize.x, 1f, waterSize.y);

                Material material = new Material(sceneConfig.waterMaterial);
                material.SetVector("_Offset_World", new Vector2(sceneConfig.waterNumChunks.x - x, sceneConfig.waterNumChunks.y - y));

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
            sceneData.AddThirdPersonViewTeam(team, instanceJetGameObject[(int)team].transform.Find(sceneConfig.thirdPersonViewName));
        }
    }

    private void UpdateSceneDataAircraftCarrier() {

    }

    public Transform GetThirdPersonViewTeam(Team team) {
        return sceneData.GetThirdPersonViewTeam(team);
    }

    // -----------------------------------------------------------------
    // Find

    private void FindParentsObjects() {
        jetsParentObject = transform.Find(sceneConfig.nameJetParentObject).gameObject;
        aircraftCarrierParentObject = transform.Find(sceneConfig.nameAircraftCarrierParentObject).gameObject;
        missileParentObject = transform.Find(sceneConfig.nameMissileParentObject).gameObject;
        waterParentObject = transform.Find(sceneConfig.nameWaterParentObject).gameObject;
        boxParentObject = transform.Find(sceneConfig.nameBoxParentObject).gameObject;
    }

    // -----------------------------------------------------------------
    // Getters and Setters

    public static void SetNextIdScene(int id) {
        nextIdScene = id;
    }

    public static int GetNextIdScene() {
        return nextIdScene;
    }

    public SceneConfig GetSceneConfig() {
        return sceneConfig;
    }

    public GameObject GetInstancedJetGameObject(Team team) {
        return instanceJetGameObject[(int)team];
    }
}
