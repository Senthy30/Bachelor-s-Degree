using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SceneConfig : ScriptableObject {

    private float sizePlane = 10f;

    public Resolution resolution;
    public Vector3Int sceneSize;
    public float margin;
    public float yAircraftCarrier = -20.5f;
    public GameObject scenePrefab;
    public GameObject boxParentObject;

    [Header("Enemies Generation")]

    public bool showChunksEnemies;
    public int minDistanceChunksEnemies;
    public Vector2Int numChunksEnemiesSpawn;
    public Material chunkMaterial;

    [Header("Aircraft Carrier Generation")]

    public GameObject aircraftCarrierGameObject;

    [Header("Jet Generation")]

    public List<GameObject> teamJetGameObject = new List<GameObject>();
    public List<Texture2D> teamJetTexture;

    [Header("Water Generation")]

    public Vector2Int waterNumChunks;
    public Material waterMaterial;
    public Material waterMaterialLowResolution;

    [Header("Decoy")]

    public int numDecoysPerJet;
    public GameObject decoyPrefab;

    [Header("Names")]

    public string modelObjectName = "Model";
    public string lowResolutionObjectName = "Low Resolution";
    public string highResolutionObjectName = "High Resolution";
    public string collisionParentObjectName = "Collision";

    public string firstPersonViewName = "First Person View";
    public string thirdPersonViewName = "Third Person View";
    public string decoySpawnPointName = "Decoy Spawn Point";

    public string nameJetSpawnPointObject = "Jet Spawn Point";
    public string nameJetParentObject = "Jets";

    public string nameWaterParentObject = "Water";

    public string nameAircraftCarrierParentObject = "Aircraft Carrier";

    public string nameMissileParentObject = "Missiles";
    public string nameMissileStorageParentObject = "Missile Storage";

    public string nameDecoyParentObject = "Decoys";

    public string nameBoxParentObject = "Box";

    public string nameEnemiesChunks = "Enemies Chunks";

    // Layers 

    public string WHEEL_LAYER_NAME = "Wheel";
    public string AIRCRAFT_RUNWAY_LAYER_NAME = "AircraftRunway";
    public string DECOY_LAYER_NAME = "Decoy";

    public float GetSizePlane() {
        return sizePlane;
    }
}
