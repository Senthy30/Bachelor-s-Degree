using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WaterDataUtils {
    
    public static void InstantiateWater(Transform waterParentTransform, SceneConfig sceneConfig) {
        if (sceneConfig.resolution == Resolution.LOW_RESOLUTION) {
            BuildLowResolutionWater(waterParentTransform, sceneConfig);
        } else {
            BuildHighResolutionWater(waterParentTransform, sceneConfig);
        }
    }

    public static GameObject GetWaterObjectFromScene(GameObject sceneObject, SceneConfig sceneConfig) {
        return sceneObject.transform.Find(sceneConfig.nameWaterParentObject).gameObject;
    }

    private static void BuildLowResolutionWater(Transform waterParentTransform, SceneConfig sceneConfig) {
        GameObject waterObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterObject.gameObject.name = "WaterChunk";
        waterObject.transform.parent = waterParentTransform;
        waterObject.transform.localPosition = Vector3.zero;
        waterObject.transform.localScale = new Vector3(sceneConfig.sceneSize.x, 0f, sceneConfig.sceneSize.z);

        waterObject.GetComponent<Renderer>().sharedMaterial = new Material(sceneConfig.waterMaterialLowResolution);
    }

    private static void BuildHighResolutionWater(Transform waterParentTransform, SceneConfig sceneConfig) {
        const float sizePlane = 10f;
        Vector2 waterSize = new Vector2(
            1f * sceneConfig.sceneSize.x / sceneConfig.waterNumChunks.x, 
            1f * sceneConfig.sceneSize.z / sceneConfig.waterNumChunks.y
        );
        Vector3 centerValueTemp = new Vector3(waterSize.x - sceneConfig.sceneSize.x, 0f, waterSize.y - sceneConfig.sceneSize.z);
        Vector3 centerFirstChunk = centerValueTemp * sizePlane / 2f;

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

}
