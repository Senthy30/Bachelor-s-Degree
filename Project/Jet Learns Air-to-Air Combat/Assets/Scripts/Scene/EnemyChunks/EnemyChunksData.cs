using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChunksData {
    private static SceneConfig m_sceneConfig;

    private GameObject m_object;

    public EnemyChunksData(GameObject parentEnemyChunk) {
        if (parentEnemyChunk.transform.Find(m_sceneConfig.nameEnemiesChunks) != null)
            m_object = parentEnemyChunk.transform.Find(m_sceneConfig.nameEnemiesChunks).gameObject;
        BuildEnemiesChunks(parentEnemyChunk);
    }

    private void BuildEnemiesChunks(GameObject parentEnemyChunk) {
        const float sizePlane = 10f;

        if (m_object != null)
            Object.DestroyImmediate(m_object);

        if (!m_sceneConfig.showChunksEnemies)
            return;

        Vector2Int sceneLW = new Vector2Int(m_sceneConfig.sceneSize.x, m_sceneConfig.sceneSize.z);
        Vector2 length = ((Vector2)(sceneLW) * sizePlane - 2 * m_sceneConfig.margin * Vector2.one) / sizePlane;
        Vector2 chunkSize = new Vector2(
            length.x / m_sceneConfig.numChunksEnemiesSpawn.x,
            length.y / m_sceneConfig.numChunksEnemiesSpawn.y
        );
        Vector3 centerFirstChunk = new Vector3(
            chunkSize.x - length.x,
            0f,
            chunkSize.y - length.y
        ) * sizePlane / 2f;

        GameObject parentObj = new GameObject(m_sceneConfig.nameEnemiesChunks);

        parentObj.transform.parent = parentEnemyChunk.transform;
        parentObj.transform.localPosition = Vector3.zero;

        for (int x = 0; x < m_sceneConfig.numChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < m_sceneConfig.numChunksEnemiesSpawn.y; y++) {
                GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Plane);

                chunk.gameObject.name = "EnemyChunk_" + x + "_" + y;
                chunk.transform.parent = parentObj.transform;
                chunk.transform.localPosition = new Vector3(x * chunkSize.x, 0f, y * chunkSize.y) * sizePlane + centerFirstChunk;
                chunk.transform.localScale = new Vector3(chunkSize.x, 1f, chunkSize.y);
                chunk.GetComponent<Renderer>().sharedMaterial = new Material(m_sceneConfig.chunkMaterial);

                if ((x + y) % 2 == 0)
                    chunk.GetComponent<Renderer>().sharedMaterial.color = Color.black;
                else chunk.GetComponent<Renderer>().sharedMaterial.color = Color.white;
            }
        }

        m_object = parentObj;
    }

    public static void SetSceneConfig(SceneConfig sceneConfig) {
        m_sceneConfig = sceneConfig;
    }

}
