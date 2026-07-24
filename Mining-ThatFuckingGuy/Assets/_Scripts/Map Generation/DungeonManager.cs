using Unity.VisualScripting;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private Texture2D dungeonMap;
    [SerializeField] private UndestructableBase unbreakablePrefab;
    [SerializeField] private DestructableSO[] destructableData;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private GameObject wallPrefab;

    private BlockData[,] blocks;
    public InstancedDropRenderer instancedDropRenderer;
    void Awake()
    {
        instancedDropRenderer = GetComponent<InstancedDropRenderer>();
        blocks = new BlockData[dungeonMap.width, dungeonMap.height];
        Debug.Log(blocks.Length);
        CreateDungeon();
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
                wall.transform.position = spawnPosition - Vector3.right * 0.7f;

                Quaternion lookRotation = Quaternion.LookRotation(Vector3.right);
                wall.transform.rotation = lookRotation;
                int randomTurn = Random.Range(0, 4);
                float turnAmount = 90 * randomTurn;

                wall.transform.parent = transform;
                wall.transform.localEulerAngles = new Vector3(wall.transform.eulerAngles.x,wall.transform.eulerAngles.y,turnAmount);
                GetPixelFromMap(pixelColor, spawnPosition, w, h, spawnPosition);
            }
        }
        for (int h = 0; h < height; h++)
            for (int w = 0; w < width; w++)
                blocks[w, h].CalculateCorners(this);
    }
    private void HandleDeathDestructable(DestructableBase breakableT)
    {
        float zRandomOffset = Random.Range(-0.5f, 0.5f);
        float yRandomOffset = Random.Range(-0.5f, 0.5f);
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
        //blocks[z, y].DebugSelf();
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