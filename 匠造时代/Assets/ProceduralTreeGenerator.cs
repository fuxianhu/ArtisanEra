using UnityEngine;
using System.Collections.Generic;

public class ProceduralTreeGenerator : MonoBehaviour
{
    [Header("树生成设置及参数")]
    public Material barkMaterial;
    public Material leafMaterial;
    public GameObject[] treePrefabs; // 预制的树Prefab数组

    [Header("碰撞体设置")]
    public bool addColliders = true;
    public ColliderType colliderType = ColliderType.Capsule;
    public bool isTrigger = false;
    public PhysicMaterial physicMaterial;

    [Header("材质设置")]
    public bool overrideMaterials = true;
    public bool applyToChildren = true;

    [Header("随机化设置")]
    public bool randomizeScale = true;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public bool randomizeRotation = true;

    [Header("其他参数")]
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
            Debug.LogWarning("ProceduralTreeGenerator 实例已存在，销毁重复实例。");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        random = new System.Random();
    }

    // 使用预制的树Prefab
    public GameObject GenerateATree(Vector3 position)
    {
        if (treePrefabs != null && treePrefabs.Length > 0)
        {
            treeCount++;

            // 随机选择一个树Prefab
            int prefabIndex = random.Next(0, treePrefabs.Length);
            GameObject treeObj = Instantiate(treePrefabs[prefabIndex], position, Quaternion.identity);
            treeObj.name = $"Tree_{treeCount}";

            // 应用随机变换
            ApplyRandomTransform(treeObj);

            // 添加碰撞体
            if (addColliders)
            {
                AddColliderToTree(treeObj);
            }

            // 应用材质
            if (overrideMaterials && (barkMaterial != null || leafMaterial != null))
            {
                ApplyMaterialsToTree(treeObj);
            }

            return treeObj;
        }
        else
        {
            throw new System.Exception("没有设置树Prefab，无法生成树。");
        }
    }

    // 应用随机变换
    private void ApplyRandomTransform(GameObject treeObj)
    {
        // 随机缩放
        if (randomizeScale)
        {
            float scale = minScale + (float)random.NextDouble() * (maxScale - minScale);
            treeObj.transform.localScale = Vector3.one * scale;
        }

        // 随机旋转
        if (randomizeRotation)
        {
            float rotationY = (float)random.NextDouble() * 360f;
            treeObj.transform.Rotate(0, rotationY, 0);
        }
    }

    // 添加碰撞体到树
    private void AddColliderToTree(GameObject treeObj)
    {
        // 先检查是否已有碰撞体
        if (treeObj.GetComponent<Collider>() != null)
        {
            Debug.Log($"树 {treeObj.name} 已有碰撞体，跳过添加");
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

        // 设置胶囊碰撞体参数
        collider.height = bounds.size.y;
        collider.radius = bounds.size.x * 0.3f; // 宽度的一部分作为半径
        collider.center = Vector3.up * bounds.extents.y;
        collider.direction = 1; // Y轴方向

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
            collider.convex = true; // 凸包碰撞，性能更好

            SetupColliderProperties(collider);
        }
        else
        {
            // 回退到胶囊碰撞体
            Debug.LogWarning($"树 {treeObj.name} 没有MeshFilter，使用胶囊碰撞体");
            AddCapsuleCollider(treeObj, treeObj.GetComponent<Renderer>().bounds);
        }
    }

    private void AddCompoundColliders(GameObject treeObj)
    {
        // 为树干和树冠分别添加碰撞体
        Renderer renderer = treeObj.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        // 树干碰撞体（胶囊）
        CapsuleCollider trunkCollider = treeObj.AddComponent<CapsuleCollider>();
        trunkCollider.height = bounds.size.y * 0.6f;
        trunkCollider.radius = bounds.size.x * 0.2f;
        trunkCollider.center = Vector3.up * bounds.size.y * 0.3f;
        trunkCollider.direction = 1;
        SetupColliderProperties(trunkCollider);

        // 树冠碰撞体（球体）
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

    // 应用材质到树
    private void ApplyMaterialsToTree(GameObject treeObj)
    {
        if (applyToChildren)
        {
            // 应用到所有子对象的Renderer
            Renderer[] renderers = treeObj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                ApplyMaterialToRenderer(renderer);
            }
        }
        else
        {
            // 只应用到根对象
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

        // 根据对象名称判断是树干还是树叶
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
            // 默认处理：如果有多个材质，分别设置
            Material[] materials = renderer.materials;
            bool materialsChanged = false;

            for (int i = 0; i < materials.Length; i++)
            {
                // 根据现有材质名称或属性判断
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

    // 批量生成树木的方法
    public List<GameObject> GenerateTreesInArea(Vector3 center, float radius, int count)
    {
        List<GameObject> generatedTrees = new();

        for (int i = 0; i < count; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 position = center + new Vector3(randomCircle.x, 0, randomCircle.y);

            // 检查位置是否合适（避免重叠）
            if (!Physics.CheckSphere(position, 1.5f))
            {
                GameObject tree = GenerateATree(position);
                generatedTrees.Add(tree);
            }
        }

        return generatedTrees;
    }

    // 设置物理材质的方法
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

    // 清除所有生成的树
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