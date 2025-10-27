using UnityEngine;
using System.Collections.Generic;

public class ProceduralTreeGenerator : MonoBehaviour
{
    [Header("���������ü�����")]
    public Material barkMaterial;
    public Material leafMaterial;
    public GameObject[] treePrefabs; // Ԥ�Ƶ���Prefab����

    [Header("��ײ������")]
    public bool addColliders = true;
    public ColliderType colliderType = ColliderType.Capsule;
    public bool isTrigger = false;
    public PhysicMaterial physicMaterial;

    [Header("��������")]
    public bool overrideMaterials = true;
    public bool applyToChildren = true;

    [Header("���������")]
    public bool randomizeScale = true;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public bool randomizeRotation = true;

    [Header("��������")]
    public int treeCount = 0;
    public static ProceduralTreeGenerator instance;

    private System.Random random;

    public enum ColliderType
    {
        Capsule,
        Box,
        Mesh,
        Compound
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("ProceduralTreeGenerator ʵ���Ѵ��ڣ������ظ�ʵ����");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        random = new System.Random();
    }

    // ʹ��Ԥ�Ƶ���Prefab
    public GameObject GenerateATree(Vector3 position)
    {
        if (treePrefabs != null && treePrefabs.Length > 0)
        {
            treeCount++;

            // ���ѡ��һ����Prefab
            int prefabIndex = random.Next(0, treePrefabs.Length);
            GameObject treeObj = Instantiate(treePrefabs[prefabIndex], position, Quaternion.identity);
            treeObj.name = $"Tree_{treeCount}";

            // Ӧ������任
            ApplyRandomTransform(treeObj);

            // �����ײ��
            if (addColliders)
            {
                AddColliderToTree(treeObj);
            }

            // Ӧ�ò���
            if (overrideMaterials && (barkMaterial != null || leafMaterial != null))
            {
                ApplyMaterialsToTree(treeObj);
            }

            return treeObj;
        }
        else
        {
            throw new System.Exception("û��������Prefab���޷���������");
        }
    }

    // Ӧ������任
    private void ApplyRandomTransform(GameObject treeObj)
    {
        // �������
        if (randomizeScale)
        {
            float scale = minScale + (float)random.NextDouble() * (maxScale - minScale);
            treeObj.transform.localScale = Vector3.one * scale;
        }

        // �����ת
        if (randomizeRotation)
        {
            float rotationY = (float)random.NextDouble() * 360f;
            treeObj.transform.Rotate(0, rotationY, 0);
        }
    }

    // �����ײ�嵽��
    private void AddColliderToTree(GameObject treeObj)
    {
        // �ȼ���Ƿ�������ײ��
        if (treeObj.GetComponent<Collider>() != null)
        {
            Debug.Log($"�� {treeObj.name} ������ײ�壬�������");
            return;
        }

        Renderer renderer = treeObj.GetComponent<Renderer>();
        if (renderer == null) return;

        Bounds bounds = renderer.bounds;

        switch (colliderType)
        {
            case ColliderType.Capsule:
                AddCapsuleCollider(treeObj, bounds);
                break;
            case ColliderType.Box:
                AddBoxCollider(treeObj, bounds);
                break;
            case ColliderType.Mesh:
                AddMeshCollider(treeObj);
                break;
            case ColliderType.Compound:
                AddCompoundColliders(treeObj);
                break;
        }
    }

    private void AddCapsuleCollider(GameObject treeObj, Bounds bounds)
    {
        CapsuleCollider collider = treeObj.AddComponent<CapsuleCollider>();

        // ���ý�����ײ�����
        collider.height = bounds.size.y;
        collider.radius = bounds.size.x * 0.3f; // ��ȵ�һ������Ϊ�뾶
        collider.center = Vector3.up * bounds.extents.y;
        collider.direction = 1; // Y�᷽��

        SetupColliderProperties(collider);
    }

    private void AddBoxCollider(GameObject treeObj, Bounds bounds)
    {
        BoxCollider collider = treeObj.AddComponent<BoxCollider>();

        collider.size = new Vector3(bounds.size.x * 0.8f, bounds.size.y, bounds.size.z * 0.8f);
        collider.center = Vector3.up * bounds.extents.y;

        SetupColliderProperties(collider);
    }

    private void AddMeshCollider(GameObject treeObj)
    {
        MeshFilter meshFilter = treeObj.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            MeshCollider collider = treeObj.AddComponent<MeshCollider>();
            collider.sharedMesh = meshFilter.sharedMesh;
            collider.convex = true; // ͹����ײ�����ܸ���

            SetupColliderProperties(collider);
        }
        else
        {
            // ���˵�������ײ��
            Debug.LogWarning($"�� {treeObj.name} û��MeshFilter��ʹ�ý�����ײ��");
            AddCapsuleCollider(treeObj, treeObj.GetComponent<Renderer>().bounds);
        }
    }

    private void AddCompoundColliders(GameObject treeObj)
    {
        // Ϊ���ɺ����ڷֱ������ײ��
        Renderer renderer = treeObj.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        // ������ײ�壨���ң�
        CapsuleCollider trunkCollider = treeObj.AddComponent<CapsuleCollider>();
        trunkCollider.height = bounds.size.y * 0.6f;
        trunkCollider.radius = bounds.size.x * 0.2f;
        trunkCollider.center = Vector3.up * bounds.size.y * 0.3f;
        trunkCollider.direction = 1;
        SetupColliderProperties(trunkCollider);

        // ������ײ�壨���壩
        SphereCollider foliageCollider = treeObj.AddComponent<SphereCollider>();
        foliageCollider.center = Vector3.up * bounds.size.y * 0.8f;
        foliageCollider.radius = Mathf.Max(bounds.size.x, bounds.size.z) * 0.4f;
        SetupColliderProperties(foliageCollider);
    }

    private void SetupColliderProperties(Collider collider)
    {
        collider.isTrigger = isTrigger;

        if (physicMaterial != null)
        {
            collider.material = physicMaterial;
        }
    }

    // Ӧ�ò��ʵ���
    private void ApplyMaterialsToTree(GameObject treeObj)
    {
        if (applyToChildren)
        {
            // Ӧ�õ������Ӷ����Renderer
            Renderer[] renderers = treeObj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                ApplyMaterialToRenderer(renderer);
            }
        }
        else
        {
            // ֻӦ�õ�������
            Renderer renderer = treeObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                ApplyMaterialToRenderer(renderer);
            }
        }
    }

    private void ApplyMaterialToRenderer(Renderer renderer)
    {
        if (renderer == null) return;

        string rendererName = renderer.gameObject.name.ToLower();

        // ���ݶ��������ж������ɻ�����Ҷ
        if (rendererName.Contains("bark") || rendererName.Contains("trunk") || rendererName.Contains("stem"))
        {
            if (barkMaterial != null)
            {
                renderer.material = barkMaterial;
            }
        }
        else if (rendererName.Contains("leaf") || rendererName.Contains("foliage"))
        {
            if (leafMaterial != null)
            {
                renderer.material = leafMaterial;
            }
        }
        else
        {
            // Ĭ�ϴ�������ж�����ʣ��ֱ�����
            Material[] materials = renderer.materials;
            bool materialsChanged = false;

            for (int i = 0; i < materials.Length; i++)
            {
                // �������в������ƻ������ж�
                if (ShouldBeBarkMaterial(materials[i]))
                {
                    materials[i] = barkMaterial;
                    materialsChanged = true;
                }
                else if (ShouldBeLeafMaterial(materials[i]))
                {
                    materials[i] = leafMaterial;
                    materialsChanged = true;
                }
            }

            if (materialsChanged)
            {
                renderer.materials = materials;
            }
        }
    }

    private bool ShouldBeBarkMaterial(Material material)
    {
        if (material == null) return false;

        string materialName = material.name.ToLower();
        return materialName.Contains("bark") || materialName.Contains("wood") ||
               materialName.Contains("trunk");// || material.color.grayscale < 0.5f;
    }

    private bool ShouldBeLeafMaterial(Material material)
    {
        if (material == null) return false;

        string materialName = material.name.ToLower();
        return materialName.Contains("leaf") || materialName.Contains("foliage") ||
               materialName.Contains("green");// || material.color.g > 0.5f;
    }

    // ����������ľ�ķ���
    public List<GameObject> GenerateTreesInArea(Vector3 center, float radius, int count)
    {
        List<GameObject> generatedTrees = new();

        for (int i = 0; i < count; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 position = center + new Vector3(randomCircle.x, 0, randomCircle.y);

            // ���λ���Ƿ���ʣ������ص���
            if (!Physics.CheckSphere(position, 1.5f))
            {
                GameObject tree = GenerateATree(position);
                generatedTrees.Add(tree);
            }
        }

        return generatedTrees;
    }

    // ����������ʵķ���
    public void SetTreePhysics(float bounciness, float friction, float dynamicFriction = 0.6f)
    {
        if (physicMaterial == null)
        {
            physicMaterial = new PhysicMaterial();
        }

        physicMaterial.bounciness = bounciness;
        physicMaterial.staticFriction = friction;
        physicMaterial.dynamicFriction = dynamicFriction;
    }

    // ����������ɵ���
    public void ClearAllTrees()
    {
        GameObject[] trees = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in trees)
        {
            if (obj.name.StartsWith("Tree_"))
            {
                Destroy(obj);
            }
        }
        treeCount = 0;
    }
}