using System.Collections;
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
    [SerializeField] private int bouncePrewarmCountPerType = 3;
    [SerializeField] private float bounceDuration = 0.15f;
    [SerializeField] private float deathDuration = .35f;
    [SerializeField] private AnimationCurve deathCurve;
    [SerializeField] private float bounceScaleMultiplier = 1.2f;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject blackDust;
    private BlockData[,] blocks;
    public InstancedDropRenderer instancedDropRenderer;
    private Dictionary<DropType, GenericObjectPool<Transform>> dropProxyPools = new();
    private Dictionary<ParticleBase, GenericObjectPool<ParticleBase>> dirtParticlePools = new();
    private Dictionary<DestructableSO, GenericObjectPool<Transform>> bouncePools = new();
    private static PlayerController player;

    void Awake()
    {
        instancedDropRenderer = GetComponent<InstancedDropRenderer>();
        player = FindAnyObjectByType<PlayerController>();
        blocks = new BlockData[dungeonMap.width, dungeonMap.height];
        CreateDungeon();
    }

    private GenericObjectPool<Transform> GetOrCreateBouncePool(DestructableSO type, Mesh mesh, Material material)
    {
        if (!bouncePools.TryGetValue(type, out var pool))
        {
            pool = new GenericObjectPool<Transform>(
                factory: () =>
                {
                    GameObject go = new GameObject($"BounceProxy_{type.name}");
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    go.AddComponent<MeshRenderer>().sharedMaterial = material;
                    go.transform.parent = transform;
                    return go.transform;
                },
                prewarmCount: bouncePrewarmCountPerType
            );
            bouncePools[type] = pool;
        }
        return pool;
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

                GameObject wall = Instantiate(wallPrefab);
                wall.transform.position = spawnPosition - Vector3.right;
                Quaternion lookRotation = Quaternion.LookRotation(Vector3.right);
                wall.transform.rotation = lookRotation;
                int randomTurn = Random.Range(0, 4);
                float turnAmount = 90 * randomTurn;
                wall.transform.parent = transform;
                wall.transform.localEulerAngles = new Vector3(wall.transform.eulerAngles.x, wall.transform.eulerAngles.y, turnAmount);
                GetPixelFromMap(pixelColor, spawnPosition, w, h, spawnPosition, wall);
            }
        }
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                blocks[w, h].CalculateCorners(this);
                if (blocks[w, h].IsEmpty)
                {
                    RevealSurroundingDust(blocks[w, h]);
                }

            }
        }
    }

    private void HandleHitDestructable(DestructableBase hitObject, bool isDead)
    {
        if (!isDead)
        {
            PlayBounceAnimation(hitObject);
            return;
        }

        BlockData ownData = blocks[(int)hitObject.transform.position.z - (int)transform.position.z, (int)hitObject.transform.position.y - (int)transform.position.y];
        ownData.IsEmpty = true;
        ownData.CalculateCorners(this);
        RecalculateNeighborCorners(ownData);
        RevealSurroundingDust(ownData);

        int aboveY = ownData.YIndex + 1;
        for (int i = aboveY; i < blocks.GetLength(1); i++)
        {
            BlockData above = blocks[ownData.ZIndex, i];
            if (above != null && above.DropsOnBlock.Count > 0)
            {
                foreach (var dropRef in above.DropsOnBlock)
                {
                    instancedDropRenderer.UngroundDrop(dropRef.Data.DropType, dropRef.DropIndex);
                }
                above.DropsOnBlock.Clear();
            }
        }

        hitObject.OnHit -= HandleHitDestructable;

        Backpack backpack = player.GetBackpack();
        if (backpack.IsEmpty() && hitObject.Data.DirtParticleVFX != null)
        {
            ParticleBase vfxType = hitObject.Data.DirtParticleVFX;
            GenericObjectPool<ParticleBase> dirtPool = GetOrCreateDirtPool(vfxType);
            ParticleBase p = dirtPool.Get();
            p.SetPool(dirtPool);
            p.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", hitObject.Data.Color);
            p.PlayAnimation(hitObject.transform.position, backpack.transform, hitObject.Data.DirtValue, (amount) =>
            {
                backpack.AddDirt(amount);
                player.AddCollectedDirt(hitObject.Data, amount); // vfxType yerine hitObject.Data
            });
        }

        if (hitObject is DropableDestructable dropable)
        {
            Vector3 spawnPos = hitObject.transform.position;
            instancedDropRenderer.RegisterDrop(dropable.DropData, dropable.DropData.Material, spawnPos);
        }
        PlayDeathAnimation(hitObject);
    }
    private void GetPixelFromMap(Color mapColor, Vector3 spawnPosition, int zIndex, int yIndex, Vector3 worldPos, GameObject wall)
    {
        DestructableBase abstractBlock = null;
        switch (GetTypeFromPixel(mapColor))
        {
            case ObjectType.FreeSpace:
                blocks[zIndex, yIndex] = new BlockData(zIndex, yIndex, true, worldPos, this);
                return;
            case ObjectType.Undestructable:
                UndestructableBase g = Instantiate(unbreakablePrefab, spawnPosition, Quaternion.identity);
                g.transform.parent = transform;
                blocks[zIndex, yIndex] = new BlockData(zIndex, yIndex, false, worldPos, this);
                GameObject dust2 = Instantiate(blackDust, worldPos + Vector3.right / 2 * 1.15f, Quaternion.Euler(0, -90, 0));
                blocks[zIndex, yIndex].BlackDust = dust2;
                blocks[zIndex, yIndex].wall = wall;
                return;
            case ObjectType.Destructable:
                abstractBlock = Instantiate(destructableData[0].Prefab, spawnPosition, Quaternion.identity);
                break;
            case ObjectType.Dirt:
                abstractBlock = Instantiate(destructableData[1].Prefab, spawnPosition, Quaternion.identity);

                break;
            case ObjectType.DirtWithGrass:
                abstractBlock = Instantiate(destructableData[2].Prefab, spawnPosition, Quaternion.identity);
                break;
        }
        abstractBlock.OnSpawned();
        abstractBlock.transform.parent = transform;
        abstractBlock.OnHit += HandleHitDestructable;
        blocks[zIndex, yIndex] = new BlockData(zIndex, yIndex, false, worldPos, this);
        GameObject dust = Instantiate(blackDust, worldPos + Vector3.right / 2 * 1.15f, Quaternion.Euler(0, -90, 0));
        blocks[zIndex, yIndex].BlackDust = dust;
        blocks[zIndex, yIndex].CurrentBlock = abstractBlock;
        blocks[zIndex, yIndex].wall = wall;

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
    private void PlayBounceAnimation(DestructableBase hitObject)
    {
        GenericObjectPool<Transform> pool = GetOrCreateBouncePool(hitObject.Data, hitObject.Data.VisualMesh.sharedMesh, hitObject.Data.VisualMaterial);
        Transform proxy = pool.Get();
        proxy.SetPositionAndRotation(hitObject.transform.position, hitObject.transform.rotation);
        proxy.localScale = Vector3.one;

        hitObject.SetVisualVisible(false);
        StartCoroutine(BounceRoutine(proxy, pool, hitObject));
    }
    private void PlayDeathAnimation(DestructableBase hitObject)
    {
        GenericObjectPool<Transform> pool = GetOrCreateBouncePool(hitObject.Data, hitObject.Data.VisualMesh.sharedMesh, hitObject.Data.VisualMaterial);
        Transform proxy = pool.Get();
        proxy.SetPositionAndRotation(hitObject.transform.position, Quaternion.identity);
        proxy.localScale = Vector3.one;

        hitObject.SetVisualVisible(false);
        Destroy(hitObject.gameObject);
        StartCoroutine(DeathAnimation(proxy, pool));
    }

    private IEnumerator BounceRoutine(Transform proxy, GenericObjectPool<Transform> pool, DestructableBase hitObject)
    {
        float elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            float scaleFactor = 1f + Mathf.Sin(t * Mathf.PI) * (bounceScaleMultiplier - 1f);
            proxy.localScale = Vector3.one * scaleFactor;
            yield return null;
        }

        proxy.localScale = Vector3.one;
        pool.Release(proxy);

        if (hitObject != null)
            hitObject.SetVisualVisible(true);
    }
    private IEnumerator DeathAnimation(Transform proxy, GenericObjectPool<Transform> pool)
    {
        float elapsed = 0f;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathDuration;
            t = deathCurve.Evaluate(t);
            proxy.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }
        pool.Release(proxy);

    }
    private void RevealSurroundingDust(BlockData block)
    {
        foreach (var (dz, dy) in Full3x3Offsets)
        {
            int nz = block.ZIndex + dz;
            int ny = block.YIndex + dy;

            if (nz < 0 || ny < 0 || nz >= blocks.GetLength(0) || ny >= blocks.GetLength(1))
                continue;

            BlockData neighbor = blocks[nz, ny];
            if (neighbor != null && neighbor.BlackDust != null)
            {
                neighbor.BlackDust.SetActive(false);
                if (neighbor.CurrentBlock != null)
                    neighbor.CurrentBlock.gameObject.SetActive(true);
                if (neighbor.wall != null)
                    neighbor.wall.SetActive(true);
            }
        }
    }
    [ContextMenu("Debug")]
    public void DebugBlocks()
    {
        foreach (var a in blocks)
        {
            a.DebugSelf();
        }
    }
    private static readonly (int z, int y)[] Full3x3Offsets =
{
    (-1, -1), (0, -1), (1, -1),
    (-1,  0), (0,  0), (1,  0),
    (-1,  1), (0,  1), (1,  1),
};
}

public enum ObjectType
{
    FreeSpace,
    Undestructable,
    Destructable,
    Dirt,
    DirtWithGrass
}