using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private Texture2D dungeonMap;
    [SerializeField] private UndestructableBase unbreakablePrefab;
    [SerializeField] private DestructableSO[] destructableData;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private int proxyPrewarmCountPerType = 5;
    [SerializeField] private int dirtParticlePrewarmCount = 10;

    private BlockData[,] blocks;
    public InstancedDropRenderer instancedDropRenderer;
    private Dictionary<DropType, GenericObjectPool<Transform>> dropProxyPools = new();
    private Dictionary<ParticleBase, GenericObjectPool<ParticleBase>> dirtParticlePools = new();
    private static PlayerController player;

    void Awake()
    {
        instancedDropRenderer = GetComponent<InstancedDropRenderer>();
        player = FindAnyObjectByType<PlayerController>();
        blocks = new BlockData[dungeonMap.width, dungeonMap.height];
        CreateDungeon();
    }


    private GenericObjectPool<Transform> GetOrCreateProxyPool(DropType type)
    {
        if (!dropProxyPools.TryGetValue(type, out var pool))
        {
            instancedDropRenderer.TryGetMeshAndMaterial(type, out Mesh mesh, out Material material);

            pool = new GenericObjectPool<Transform>(
                factory: () =>
                {
                    GameObject go = new GameObject($"CollectProxy_{type}");
                    go.AddComponent<MeshFilter>();
                    go.AddComponent<MeshRenderer>();
                    go.transform.parent = transform;
                    return go.transform;
                },
                prewarmCount: proxyPrewarmCountPerType,
                onGet: (t) =>
                {
                    t.GetComponent<MeshFilter>().sharedMesh = mesh;
                    t.GetComponent<MeshRenderer>().sharedMaterial = material;
                }
            );
            dropProxyPools[type] = pool;
        }
        return pool;
    }

    public Transform CheckoutDropProxy(DropType type, Vector3 position)
    {
        Transform proxy = GetOrCreateProxyPool(type).Get();
        proxy.SetPositionAndRotation(position, Quaternion.identity);
        return proxy;
    }

    public void ReturnDropProxy(DropType type, Transform proxy)
    {
        if (dropProxyPools.TryGetValue(type, out var pool))
        {
            pool.Release(proxy);
        }
    }

    // ---------------- Dirt Particle Pooling ----------------

    private GenericObjectPool<ParticleBase> GetOrCreateDirtPool(ParticleBase prefab)
    {
        if (!dirtParticlePools.TryGetValue(prefab, out var pool))
        {
            pool = new GenericObjectPool<ParticleBase>(
                factory: () => Instantiate(prefab, transform),
                prewarmCount: dirtParticlePrewarmCount
            );
            dirtParticlePools[prefab] = pool;
        }
        return pool;
    }

    // ---------------- Dungeon Oluşturma ----------------

    private void CreateDungeon()
    {
        int width = dungeonMap.width;
        int height = dungeonMap.height;
        Color32[] pixels = dungeonMap.GetPixels32();
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                Color pixelColor = pixels[h * width + w];
                Vector3 spawnPosition = transform.position + new Vector3(0, h, w);
                GetPixelFromMap(pixelColor, spawnPosition, w, h, spawnPosition);
            }
        }
        for (int h = 0; h < height; h++)
            for (int w = 0; w < width; w++)
                blocks[w, h].CalculateCorners(this);
    }

    private void HandleDeathDestructable(DestructableBase breakableT)
    {
        BlockData ownData = blocks[(int)breakableT.transform.position.z - (int)transform.position.z, (int)breakableT.transform.position.y - (int)transform.position.y];
        ownData.IsEmpty = true;
        ownData.CalculateCorners(this);
        RecalculateNeighborCorners(ownData);

        int aboveY = ownData.YIndex + 1;
        for (int i = aboveY; i < blocks.GetLength(1); i++)
        {
            BlockData above = blocks[ownData.ZIndex, i];
            if (above != null && above.DropsOnBlock.Count > 0)
            {
                foreach (var dropRef in above.DropsOnBlock)
                {
                    instancedDropRenderer.UngroundDrop(dropRef.DropType, dropRef.DropIndex);
                }
                above.DropsOnBlock.Clear();
            }
        }

        breakableT.OnDeath -= HandleDeathDestructable;

        // Dirt spawn kararı ve mekanizması artık burada, DestructableBase'in hiç haberi yok
        Backpack backpack = player.GetBackpack();
        if (backpack.IsEmpty() && breakableT.Data.DirtParticleVFX != null)
        {
            ParticleBase vfxType = breakableT.Data.DirtParticleVFX;
            GenericObjectPool<ParticleBase> pool = GetOrCreateDirtPool(breakableT.Data.DirtParticleVFX);
            ParticleBase p = pool.Get();
            p.SetPool(pool);
            p.PlayAnimation(breakableT.transform.position, backpack.transform, breakableT.Data.DirtValue, (amount) =>
      {
          backpack.AddDirt(amount);
          player.AddCollectedDirt(vfxType, amount);
      });
        }
        if (breakableT is DropableDestructable dropable)
        {
            Vector3 spawnPos = breakableT.transform.position;
            instancedDropRenderer.RegisterDrop(dropable.DropData, dropable.DropData.Material, spawnPos);
        }
    }

    private void GetPixelFromMap(Color mapColor, Vector3 spawnPosition, int zIndex, int yIndex, Vector3 worldPos)
    {
        switch (GetTypeFromPixel(mapColor))
        {
            case ObjectType.FreeSpace:
                blocks[zIndex, yIndex] = new BlockData(zIndex, yIndex, true, worldPos, this);
                return;
            case ObjectType.Undestructable:
                int randomRotation2 = Random.Range(0, 4);
                Vector3 randomRotationVector2 = Vector3.zero;
                switch (randomRotation2)
                {
                    case 0: randomRotationVector2 = Vector3.zero; break;
                    case 1: randomRotationVector2 = Vector3.up * 90; break;
                    case 2: randomRotationVector2 = Vector3.up * 180; break;
                    case 3: randomRotationVector2 = Vector3.up * 270; break;
                }
                UndestructableBase g = Instantiate(unbreakablePrefab, spawnPosition, Quaternion.Euler(randomRotationVector2));
                g.transform.parent = transform;
                break;
            case ObjectType.Destructable:
                DestructableBase g2 = Instantiate(destructableData[0].Prefab, spawnPosition, Quaternion.identity);
                g2.OnSpawned();
                g2.transform.parent = transform;
                g2.OnDeath += HandleDeathDestructable;
                break;
            case ObjectType.Dirt:
                DestructableBase dirt = Instantiate(destructableData[1].Prefab, spawnPosition, Quaternion.identity);
                dirt.OnSpawned();
                dirt.transform.parent = transform;
                dirt.OnDeath += HandleDeathDestructable;
                break;
            case ObjectType.DirtWithGrass:
                DestructableBase dirtGrass = Instantiate(destructableData[2].Prefab, spawnPosition, Quaternion.identity);
                dirtGrass.OnSpawned();
                dirtGrass.transform.parent = transform;
                dirtGrass.OnDeath += HandleDeathDestructable;
                break;
        }
        blocks[zIndex, yIndex] = new BlockData(zIndex, yIndex, false, worldPos, this);
    }

    private ObjectType GetTypeFromPixel(Color c)
    {
        if (ColorApproximately(c, Color.white)) return ObjectType.FreeSpace;
        if (ColorApproximately(c, Color.black)) return ObjectType.Undestructable;
        if (ColorApproximately(c, Color.blue)) return ObjectType.Destructable;
        if (ColorApproximately(c, Color.green)) return ObjectType.DirtWithGrass;
        if (ColorApproximately(c, Color.brown)) return ObjectType.Dirt;
        Debug.LogWarning($"No color information: {c}");
        return ObjectType.FreeSpace;
    }

    private bool ColorApproximately(Color a, Color b, float tolerance = 0.05f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }

    public BlockData GetEmptyBlockFromVertical(BlockData startBlock)
    {
        int yIndex = startBlock.YIndex - 1;
        if (yIndex < 0) return null;
        return blocks[startBlock.ZIndex, yIndex];
    }

    public BlockData GetBlockFromWorldPosition(Vector3 pos, out bool isEmpty)
    {
        Vector3 localPos = pos - transform.position;
        float zPercent = localPos.z / dungeonMap.width;
        float yPercent = localPos.y / dungeonMap.height;
        zPercent = Mathf.Clamp01(zPercent);
        yPercent = Mathf.Clamp01(yPercent);
        int z = Mathf.RoundToInt(zPercent * dungeonMap.width);
        int y = Mathf.RoundToInt(yPercent * dungeonMap.height);
        z = Mathf.Clamp(z, 0, blocks.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, blocks.GetLength(1) - 1);
        isEmpty = blocks[z, y].IsEmpty;
        return blocks[z, y];
    }

    public bool GetEmptyBlockFromWorldPosition(BlockData currentCheckBlock, int zIndex, int yIndex)
    {
        int z = currentCheckBlock.ZIndex + zIndex;
        int y = currentCheckBlock.YIndex + yIndex;
        if (z >= blocks.GetLength(0) || y >= blocks.GetLength(1) || z < 0 || y < 0) return false;
        return blocks[currentCheckBlock.ZIndex + zIndex, currentCheckBlock.YIndex + yIndex].IsEmpty;
    }

    private static readonly (int z, int y)[] NeighborOffsets = { (1, 0), (0, -1), (-1, 0), (0, 1) };
    private void RecalculateNeighborCorners(BlockData block)
    {
        foreach (var (dz, dy) in NeighborOffsets)
        {
            int nz = block.ZIndex + dz;
            int ny = block.YIndex + dy;
            if (nz < 0 || ny < 0 || nz >= blocks.GetLength(0) || ny >= blocks.GetLength(1))
                continue;

            BlockData neighbor = blocks[nz, ny];
            if (neighbor != null)
                neighbor.CalculateCorners(this);
        }
    }

    public int DungeonHeight()
    {
        return dungeonMap.height;
    }

    [ContextMenu("Debug")]
    public void DebugBlocks()
    {
        foreach (var a in blocks)
        {
            a.DebugSelf();
        }
    }
}

public enum ObjectType
{
    FreeSpace,
    Undestructable,
    Destructable,
    Dirt,
    DirtWithGrass
}