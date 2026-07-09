using JetBrains.Annotations;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private Texture2D dungeonMap;
    [SerializeField] private UndestructableBase unbreakablePrefab;
    [SerializeField] private DestructableSO destructableData;
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private GameObject holder;

    private BlockData[,] blocks;
    void Awake()
    {
        blocks = new BlockData[dungeonMap.width, dungeonMap.height];
        Debug.Log(blocks.Length);
        CreateDungeon();
    }
    // void Update()
    // {
    //     Plane plane = new Plane(Vector3.right, transform.position);
    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     if (plane.Raycast(ray, out float enter))
    //     {
    //         Vector3 hitPoint = ray.GetPoint(enter);
    //         BlockData current = GetBlockFromWorldPosition(hitPoint);
    //         holder.transform.position = current.WorldPosition;
    //     }
    // }
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
    }
    private void HandleDeathDestructable(DestructableBase breakableT)
    {
        float zRandomOffset = Random.Range(-0.5f, 0.5f);
        float yRandomOffset = Random.Range(-0.5f, 0.5f);
        Instantiate(dropPrefab, breakableT.transform.position + new Vector3(0, yRandomOffset, zRandomOffset), Quaternion.identity);
        breakableT.OnDeath -= HandleDeathDestructable;
    }
    private void GetPixelFromMap(Color mapColor, Vector3 spawnPosition, int zIndex, int yIndex, Vector3 worldPos)
    {
        switch (GetTypeFromPixel(mapColor))
        {
            case ObjectType.FreeSpace:
                blocks[zIndex, yIndex] = new BlockData(zIndex, yIndex, true, worldPos);
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

                DestructableBase g2 = Instantiate(destructableData.Prefab, spawnPosition, Quaternion.identity);
                g2.OnSpawned();
                g2.transform.parent = transform;
                g2.OnDeath += HandleDeathDestructable;
                break;
        }
        blocks[zIndex, yIndex] = new BlockData(zIndex, yIndex, false, worldPos);
    }
    private ObjectType GetTypeFromPixel(Color c)
    {
        if (ColorApproximately(c, Color.white)) return ObjectType.FreeSpace;
        if (ColorApproximately(c, Color.black)) return ObjectType.Undestructable;
        if (ColorApproximately(c, Color.blue)) return ObjectType.Destructable;

        Debug.LogWarning($"No color information: {c}");
        return ObjectType.FreeSpace;
    }


    private bool ColorApproximately(Color a, Color b, float tolerance = 0.05f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
    public BlockData GetBlockFromWorldPosition(Vector3 pos)
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
        blocks[z, y].DebugSelf();
        return blocks[z, y];
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
    Destructable
}