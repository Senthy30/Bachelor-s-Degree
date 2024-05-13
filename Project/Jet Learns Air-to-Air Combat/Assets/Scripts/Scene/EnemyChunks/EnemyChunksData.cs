using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyChunksData {

    private struct ChunkData {
        public bool occupied;
        public Vector3 position;

        public ChunkData(bool occupied, Vector3 position) {
            this.occupied = occupied;
            this.position = position;
        }
    }

    public struct ComponentInChunkData {
        public Vector3Int chunkPosition;
        public Vector3 position;
        public Quaternion rotation;

        public ComponentInChunkData(Vector3Int chunkPosition, Vector3 position, Quaternion rotation) {
            this.chunkPosition = chunkPosition;
            this.position = position;
            this.rotation = rotation;
        }
    }

    private static SceneConfig m_sceneConfig;

    private Vector3 m_chunkSize;
    private GameObject m_object;
    private List<int> m_numChunksAvailableOnHeight;
    private List<List<List<ChunkData>>> m_centerPosChunksAvailable;

    private List<ComponentInChunkData> m_aircraftCarriersChunksData;
    private List<ComponentInChunkData> m_jetsChunksData;

    public EnemyChunksData(GameObject parentEnemyChunk) {
        if (parentEnemyChunk.transform.Find(m_sceneConfig.nameEnemiesChunks) != null)
            m_object = parentEnemyChunk.transform.Find(m_sceneConfig.nameEnemiesChunks).gameObject;

        if (m_object != null)
            Object.DestroyImmediate(m_object);

        GenerateEnemiesChunks();
        BuildEnemiesChunks(parentEnemyChunk);
    }


    #region Public Methods

    public ComponentInChunkData GetAircraftCarriersData(Team team) {
        return m_aircraftCarriersChunksData[(int)team];
    }

    public ComponentInChunkData GetJetData(Team team) {
        return m_jetsChunksData[(int)team];
    }

    public void GenerateAircraftCarriersChunk(int num) {
        m_aircraftCarriersChunksData = new List<ComponentInChunkData>();
        for (int index = 0; index < num; index++) {
            Vector3Int chunkPos = GenerateChunkPositionAtHeight(0);
            Vector3 position = GeneratePosition(chunkPos);
            Quaternion rotation = GenerateRotation();

            position.y = m_sceneConfig.yAircraftCarrier;
            rotation = Quaternion.Euler(
                0f,
                rotation.eulerAngles.y,
                0f
            );

            MarkPosAsOccupied(chunkPos, new Vector3Int(m_sceneConfig.minDistanceChunksEnemies, 0, m_sceneConfig.minDistanceChunksEnemies));
            m_aircraftCarriersChunksData.Add(new ComponentInChunkData(chunkPos, position, rotation));
        }

        ClearChunksAvailable();
    }

    public void GenerateJetsChunk(int num) {
        m_jetsChunksData = new List<ComponentInChunkData>();
        for (int index = 0; index < num; index++) {
            Vector3Int chunkPos = GenerateChunkPosition();
            Vector3 position = GeneratePosition(chunkPos);
            Quaternion rotation = GenerateRotation();

            MarkPosAsOccupied(chunkPos, new Vector3Int(m_sceneConfig.minDistanceChunksJets, 0, m_sceneConfig.minDistanceChunksJets));
            m_jetsChunksData.Add(new ComponentInChunkData(chunkPos, position, rotation));
        }

        ClearChunksAvailable();
    }

    public bool MarkPosAsOccupied(Vector3Int pos, Vector3Int space) {
        if (m_centerPosChunksAvailable[pos.x][pos.y][pos.z].occupied)
            return false;

        for (int x = pos.x - space.x; x <= pos.x + space.x; x++) {
            if (x < 0 || x >= m_sceneConfig.dimChunksEnemiesSpawn.x)
                continue;
            for (int y = pos.y - space.y; y <= pos.y + space.y; y++) {
                if (y < 0 || y >= m_sceneConfig.dimChunksEnemiesSpawn.y)
                    continue;
                for (int z = pos.z - space.z; z <= pos.z + space.z; z++) {
                    if (z < 0 || z >= m_sceneConfig.dimChunksEnemiesSpawn.z)
                        continue;
                    m_centerPosChunksAvailable[x][y][z] = new ChunkData(true, m_centerPosChunksAvailable[x][y][z].position);
                    m_numChunksAvailableOnHeight[y]--;
                }
            }
        }

        return true;
    }

    public bool IsPositionWithinAChunk(Vector3 position) {
        float yDistance = SceneRewards.GetRewardsConfig().distanceToFloorNCeiling;
        float xzDistance = SceneRewards.GetRewardsConfig().distanceToWall;

        float xPos = position.x + m_sceneConfig.sceneSize.x * 5;
        float zPos = position.z + m_sceneConfig.sceneSize.z * 5;

        if (xPos < xzDistance || xPos > m_sceneConfig.sceneSize.x * 10 - xzDistance)
            return false;
        if (position.y < yDistance || position.y > m_sceneConfig.sceneSize.y * 10 - yDistance)
            return false;
        if (zPos < xzDistance || zPos > m_sceneConfig.sceneSize.z * 10 - xzDistance)
            return false;

        return true;
    }

    public Vector3 GetRandomPositionWithinScene() {
        return GeneratePosition(GenerateChunkPosition());
    }

    public Quaternion GetRandomRotation() {
        return GenerateRotation();
    }

    #endregion


    #region Private Methods

    private void GenerateEnemiesChunks() {
        m_centerPosChunksAvailable = new List<List<List<ChunkData>>>();

        float yDistance = SceneRewards.GetRewardsConfig().distanceToFloorNCeiling;
        float xzDistance = SceneRewards.GetRewardsConfig().distanceToWall;

        Vector3Int sceneLHW = new Vector3Int(m_sceneConfig.sceneSize.x, m_sceneConfig.sceneSize.y, m_sceneConfig.sceneSize.z) * 10;
        Vector3 sceneLength = new Vector3(
            sceneLHW.x - 2 * xzDistance,
            sceneLHW.y - 2 * yDistance,
            sceneLHW.z - 2 * xzDistance
        );
        Vector3 chunkSize = new Vector3(
            sceneLength.x / m_sceneConfig.dimChunksEnemiesSpawn.x,
            sceneLength.y / m_sceneConfig.dimChunksEnemiesSpawn.y,
            sceneLength.z / m_sceneConfig.dimChunksEnemiesSpawn.z
        );
        Vector3 centerFirstChunk = new Vector3(
            (chunkSize.x - sceneLength.x) / 2f,
            chunkSize.y / 2f + yDistance,
            (chunkSize.z - sceneLength.z) / 2f
        );

        for (int x = 0; x < m_sceneConfig.dimChunksEnemiesSpawn.x; x++) {
            m_centerPosChunksAvailable.Add(new List<List<ChunkData>>());
            for (int y = 0; y < m_sceneConfig.dimChunksEnemiesSpawn.y; y++) {
                m_centerPosChunksAvailable[x].Add(new List<ChunkData>());
                for (int z = 0; z < m_sceneConfig.dimChunksEnemiesSpawn.z; z++) {
                    Vector3 centerChunk = new Vector3(x * chunkSize.x, y * chunkSize.y, z * chunkSize.z) + centerFirstChunk;
                    m_centerPosChunksAvailable[x][y].Add(new ChunkData(false, centerChunk));
                }
            }
        }

        m_numChunksAvailableOnHeight = new List<int>();
        for (int y = 0; y < m_sceneConfig.dimChunksEnemiesSpawn.y; y++) {
            m_numChunksAvailableOnHeight.Add(m_sceneConfig.dimChunksEnemiesSpawn.x * m_sceneConfig.dimChunksEnemiesSpawn.z);
        }

        m_chunkSize = chunkSize;
    }

    private void BuildEnemiesChunks(GameObject parentEnemyChunk) {
        if (!m_sceneConfig.showChunksEnemies)
            return;

        GameObject parentObj = new GameObject(m_sceneConfig.nameEnemiesChunks);

        parentObj.transform.parent = parentEnemyChunk.transform;
        parentObj.transform.localPosition = Vector3.zero;

        for (int x = 0; x < m_sceneConfig.dimChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < m_sceneConfig.dimChunksEnemiesSpawn.y; y++) {
                for (int z = 0; z < m_sceneConfig.dimChunksEnemiesSpawn.z; z++) {
                    GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    chunk.gameObject.name = "EnemyChunk_" + x + "_" + y + "_" + z;
                    chunk.transform.parent = parentObj.transform;
                    chunk.transform.localPosition = m_centerPosChunksAvailable[x][y][z].position;
                    chunk.transform.localScale = m_chunkSize;
                    chunk.GetComponent<Renderer>().sharedMaterial = new Material(m_sceneConfig.chunkMaterial);
                    Object.DestroyImmediate(chunk.GetComponent<Collider>());

                    if ((x + y + z) % 2 == 0)
                        chunk.GetComponent<Renderer>().sharedMaterial.color = Color.black;
                    else chunk.GetComponent<Renderer>().sharedMaterial.color = Color.white;
                }
            }
        }

        m_object = parentObj;
    }

    private Vector3Int GenerateChunkPosition() {
        if (m_numChunksAvailableOnHeight.Sum() <= 0) {
            Debug.LogError("No more available chunks");
            return Vector3Int.zero;
        }

        bool repeat = true;
        Vector3Int pos = Vector3Int.zero;
        while (repeat) {
            int y = UnityEngine.Random.Range(0, m_sceneConfig.dimChunksEnemiesSpawn.y);
            if (m_numChunksAvailableOnHeight[y] > 0) {
                repeat = false;
                pos = GenerateChunkPositionAtHeight(y);
            }
        }

        return pos;
    }

    private Vector3Int GenerateChunkPositionAtHeight(int y) {
        if (m_numChunksAvailableOnHeight[y] <= 0) {
            Debug.LogError("No more available chunks on height " + y);
            return Vector3Int.zero;
        }

        bool repeat;
        Vector3Int pos;
        do {
            pos = new Vector3Int(
                UnityEngine.Random.Range(0, m_sceneConfig.dimChunksEnemiesSpawn.x),
                y,
                UnityEngine.Random.Range(0, m_sceneConfig.dimChunksEnemiesSpawn.y)
            );

            repeat = m_centerPosChunksAvailable[pos.x][pos.y][pos.z].occupied;
        } while (repeat);

        return pos;
    }

    private Vector3 GeneratePosition(Vector3Int chunkPosition) {
        const float marginOfChunk = 50;

        Vector3 position = m_centerPosChunksAvailable[chunkPosition.x][chunkPosition.y][chunkPosition.z].position;
        Vector3 offset = new Vector3(
            UnityEngine.Random.Range(-m_chunkSize.x / 2f + marginOfChunk, m_chunkSize.x / 2f - marginOfChunk),
            UnityEngine.Random.Range(-m_chunkSize.y / 2f + marginOfChunk, m_chunkSize.y / 2f - marginOfChunk),
            UnityEngine.Random.Range(-m_chunkSize.z / 2f + marginOfChunk, m_chunkSize.z / 2f - marginOfChunk)
        );

        return position + offset;
    }

    private Quaternion GenerateRotation() {
        Vector3 eulerRotation = new Vector3(
            UnityEngine.Random.Range(-180f, 180f),
            UnityEngine.Random.Range(-180f, 180f),
            UnityEngine.Random.Range(-180f, 180f)
        );

        return Quaternion.Euler(eulerRotation);
    }

    private void ClearChunksAvailable() {
        for (int x = 0; x < m_sceneConfig.dimChunksEnemiesSpawn.x; x++) {
            for (int y = 0; y < m_sceneConfig.dimChunksEnemiesSpawn.y; y++) {
                for (int z = 0; z < m_sceneConfig.dimChunksEnemiesSpawn.z; z++) {
                    if (m_centerPosChunksAvailable[x][y][z].occupied) {
                        m_centerPosChunksAvailable[x][y][z] = new ChunkData(false, m_centerPosChunksAvailable[x][y][z].position);
                    }
                }
            }
        }

        m_numChunksAvailableOnHeight = new List<int>();
        for (int y = 0; y < m_sceneConfig.dimChunksEnemiesSpawn.y; y++) {
            m_numChunksAvailableOnHeight.Add(m_sceneConfig.dimChunksEnemiesSpawn.x * m_sceneConfig.dimChunksEnemiesSpawn.z);
        }
    }

    #endregion


    #region Static Methods

    public static void SetSceneConfig(SceneConfig sceneConfig) {
        m_sceneConfig = sceneConfig;
    }

    #endregion

}
