using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TerrainSettings
{
    public int width = 512;
    public int height = 512;
    public int octaves = 8;
    public float lacunarity = 2.5f;
    public float gain = 0.4f;
    public float scale = 0.05f;
    public float heightScale = 200f;
    public float amplitude = 0.1f;
    public float frequency = 0.3f;

    public TerrainSettings(int width,
        int height,
        int octaves,
        float lacunarity,
        float gain,
        float scale,
        float heightScale,
        float amplitude,
        float frequency)
    {
        this.width = width;
        this.height = height;
        this.octaves = octaves;
        this.lacunarity = lacunarity;
        this.gain = gain;
        this.scale = scale;
        this.heightScale = heightScale;
        this.amplitude = amplitude;
        this.frequency = frequency;
    }
}


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldGeneration : MonoBehaviour
{
    public GameObject player;
    public GameObject worldGenerationObject;
    public int viewDistance = 16;
    static public int chunkSize = 32;
    private int seed;

    // ���ܿ��Ʋ���
    public float updateInterval = 0.5f;
    public int chunksPerFrame = 1; // ÿ֡���ɵ���������
    private float lastUpdateTime = 0f;
    private Vector3 lastPlayerPosition;

    static public Dictionary<string, TerrainSettings> sList = new(){
        { "plain", new(512, 512, 8, 2.5f, 0.4f, 1.0f, 50f, 0.01f, 0.1f) },
        { "mountain_range", new(512, 512, 8, 2.5f, 0.4f, 1.0f, 200f, 0.01f, 0.1f) },
        { "hillside", new(512, 512, 8, 2.5f, 0.4f, 0.05f, 200f, 1f, 1f) }
    };

    public TerrainSettings s = sList["mountain_range"];

    private Dictionary<Vector2, GameObject> loadedChunks = new();
    private Queue<ChunkGenerationTask> generationQueue = new();
    private HashSet<Vector2> queuedChunks = new(); // �����Ѽ�����е�����
    private bool isUpdatingChunks = false;

    // ������������
    private class ChunkGenerationTask
    {
        public Vector2 chunkPos;
        public int priority; // ���ȼ�������ԽС���ȼ�Խ�ߣ�����Ⱦ��

        public ChunkGenerationTask(Vector2 pos, int priority)
        {
            this.chunkPos = pos;
            this.priority = priority;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            throw new System.ArgumentNullException("�� WorldGeneration ��δָ����Ҷ���");
        }

        seed = FunctionManager.instance.GetRandomInt();

        lastPlayerPosition = player.transform.position;

        //ProceduralTreeGenerator.instance.GenerateATree(new Vector3(0, 100, 0));
        //ProceduralTreeGenerator.instance.GenerateTreesInArea(new Vector3(0, 0, 0), 50f, 20);

        // ��ʼ����
        StartCoroutine(ChunkUpdateCoroutine());
    }

    private void Update()
    {
        // �������ɶ���
        ProcessGenerationQueue();

        // ����Ƿ���Ҫ��������
        if (Time.time - lastUpdateTime >= updateInterval &&
            Vector3.Distance(player.transform.position, lastPlayerPosition) > chunkSize * 0.3f)
        {
            lastUpdateTime = Time.time;
            lastPlayerPosition = player.transform.position;

            if (!isUpdatingChunks)
            {
                StartCoroutine(UpdateChunksCoroutine());
            }
        }
    }

    private IEnumerator ChunkUpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            if (!isUpdatingChunks &&
                Vector3.Distance(player.transform.position, lastPlayerPosition) > chunkSize * 0.3f)
            {
                lastPlayerPosition = player.transform.position;
                StartCoroutine(UpdateChunksCoroutine());
            }
        }
    }

    private IEnumerator UpdateChunksCoroutine()
    {
        isUpdatingChunks = true;

        Vector3 playerPos = player.transform.position;
        int centerX = Mathf.FloorToInt(playerPos.x / chunkSize);
        int centerZ = Mathf.FloorToInt(playerPos.z / chunkSize);

        // ������Ҫ���ص����飨����������
        List<Vector2> chunksToLoad = CalculateChunksByDistance(centerX, centerZ);

        // ж�س����Ӿ������
        yield return StartCoroutine(UnloadDistantChunks(chunksToLoad));

        // ����Ҫ���ɵ����鰴���ȼ��������
        EnqueueChunksForGeneration(chunksToLoad, centerX, centerZ);

        isUpdatingChunks = false;
    }

    private List<Vector2> CalculateChunksByDistance(int centerX, int centerZ)
    {
        List<Vector2> chunks = new List<Vector2>();
        int rsq = viewDistance * viewDistance;

        for (int dx = -viewDistance; dx <= viewDistance; dx++)
        {
            for (int dz = -viewDistance; dz <= viewDistance; dz++)
            {
                if (dx * dx + dz * dz <= rsq)
                {
                    chunks.Add(new Vector2(centerX + dx, centerZ + dz));
                }
            }
        }

        // ���������������Խ��������ǰ��
        chunks.Sort((a, b) =>
        {
            float distA = Mathf.Pow(a.x - centerX, 2) + Mathf.Pow(a.y - centerZ, 2);
            float distB = Mathf.Pow(b.x - centerX, 2) + Mathf.Pow(b.y - centerZ, 2);
            return distA.CompareTo(distB);
        });

        return chunks;
    }

    private IEnumerator UnloadDistantChunks(List<Vector2> chunksToKeep)
    {
        List<Vector2> chunksToRemove = new List<Vector2>();

        foreach (var chunkPos in loadedChunks.Keys)
        {
            if (!chunksToKeep.Contains(chunkPos))
            {
                chunksToRemove.Add(chunkPos);
            }
        }

        // ÿ֡ж��һ�����飬���⿨��
        foreach (var chunkPos in chunksToRemove)
        {
            if (loadedChunks.TryGetValue(chunkPos, out GameObject chunkObj))
            {
                Destroy(chunkObj);
                loadedChunks.Remove(chunkPos);

                // Ҳ�Ӷ������Ƴ���������ڣ�
                queuedChunks.Remove(chunkPos);

                yield return null;
            }
        }

        // ǿ���������գ��ͷ��ڴ�
        if (chunksToRemove.Count > 0)
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }

    private void EnqueueChunksForGeneration(List<Vector2> chunksToLoad, int centerX, int centerZ)
    {
        foreach (var chunkPos in chunksToLoad)
        {
            // �������δ������δ�ڶ����У���������
            if (!loadedChunks.ContainsKey(chunkPos) && !queuedChunks.Contains(chunkPos))
            {
                // �������ȼ����������Խ�������ȼ���ֵԽС��Խ�ȴ���
                int distance = (int)(Mathf.Pow(chunkPos.x - centerX, 2) + Mathf.Pow(chunkPos.y - centerZ, 2));
                var task = new ChunkGenerationTask(chunkPos, distance);

                generationQueue.Enqueue(task);
                queuedChunks.Add(chunkPos);
            }
        }

        // �����ȼ������������
        var sortedQueue = new Queue<ChunkGenerationTask>(
            generationQueue.OrderBy(task => task.priority)
        );
        generationQueue = sortedQueue;
    }

    private void ProcessGenerationQueue()
    {
        for (int i = 0; i < chunksPerFrame && generationQueue.Count > 0; i++)
        {
            var task = generationQueue.Dequeue();
            queuedChunks.Remove(task.chunkPos);

            if (!loadedChunks.ContainsKey(task.chunkPos))
            {
                GenerateChunkImmediate((int)task.chunkPos.x, (int)task.chunkPos.y);
            }
        }
    }

    private void GenerateChunkImmediate(int chunkX, int chunkY)
    {
        int worldX = chunkX * chunkSize;
        int worldZ = chunkY * chunkSize;

        GameObject chunkObj = Generation(worldX, worldZ);
        loadedChunks[new Vector2(chunkX, chunkY)] = chunkObj;
    }

    GameObject Generation(int vx, int vz)
    {
        GameObject chunkObj = new($"Chunk_{vx}_{vz}");
        chunkObj.transform.parent = this.transform;
        chunkObj.transform.position = new Vector3(vx, 0, vz);

        MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();

        // ʹ�ò���ʵ�������ǹ�����ʣ������ڴ�����
        if (worldGenerationObject != null)
        {
            var originalMaterial = worldGenerationObject.GetComponent<MeshRenderer>().sharedMaterial;
            mr.material = new Material(originalMaterial);
        }
        else
        {
            mr.material = new Material(Shader.Find("Standard")) { color = Color.green };
        }

        MeshCollider mc = chunkObj.AddComponent<MeshCollider>();

        // ������������
        MeshData meshData = GenerateMeshData(vx, vz);
        Mesh mesh = CreateMeshFromData(meshData);

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        return chunkObj;
    }

    private MeshData GenerateMeshData(int vx, int vz)
    {
        FastNoiseLite noise = new(seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        int vertCount = (chunkSize + 1) * (chunkSize + 1);
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[chunkSize * chunkSize * 6];

        // ���ɶ����UV
        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float worldX = vx + x;
                float worldZ = vz + z;

                float amplitude = s.amplitude;
                float frequency = s.frequency;
                float value = 0f;
                float max = 0f;

                for (int o = 0; o < s.octaves; o++)
                {
                    float nx = worldX * s.scale * frequency;
                    float nz = worldZ * s.scale * frequency;
                    value += noise.GetNoise(nx, nz) * amplitude;
                    max += amplitude;
                    amplitude *= s.gain;
                    frequency *= s.lacunarity;
                }

                value /= max;
                int idx = z * (chunkSize + 1) + x;

                vertices[idx] = new Vector3(x, value * s.heightScale, z);
                uvs[idx] = new Vector2((float)x / chunkSize, (float)z / chunkSize);

                bool IsGenerateTrees = false;
                if (IsGenerateTrees && new System.Random().Next(1, 500) == 1 && vertices[idx].y >= -100 && vertices[idx].y <= 180)
                {
                    // �ڴ˶���λ������һ����
                    Vector3 treePos = new(worldX, vertices[idx].y, worldZ);
                    ProceduralTreeGenerator.instance.GenerateATree(treePos);
                }
            }
        }

        // ����������
        int tri = 0;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int idx = z * (chunkSize + 1) + x;

                triangles[tri++] = idx;
                triangles[tri++] = idx + chunkSize + 1;
                triangles[tri++] = idx + 1;

                triangles[tri++] = idx + 1;
                triangles[tri++] = idx + chunkSize + 1;
                triangles[tri++] = idx + chunkSize + 2;
            }
        }

        return new MeshData { vertices = vertices, triangles = triangles, uvs = uvs };
    }

    private Mesh CreateMeshFromData(MeshData meshData)
    {
        Mesh mesh = new()
        {
            indexFormat = (meshData.vertices.Length > 65000) ?
                UnityEngine.Rendering.IndexFormat.UInt32 :
                UnityEngine.Rendering.IndexFormat.UInt16,
            vertices = meshData.vertices,
            triangles = meshData.triangles,
            uv = meshData.uvs
        };

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }

    // �����ṹ��
    private struct MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
    }
}