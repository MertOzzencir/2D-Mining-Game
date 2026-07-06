using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [SerializeField] private Texture2D dungeonMap;
    [SerializeField] private GameObject unbreakablePrefab;
    [SerializeField] private DestructableSO destructableData;
    [SerializeField] private GameObject dropPrefab;
    void Awake()
    {
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
                Vector3 spawnPosition = transform.position + new Vector3(0, h, w) - new Vector3(0, height / 2, 0);
                GetPixelFromMap(pixelColor, spawnPosition);
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
    private void GetPixelFromMap(Color mapColor, Vector3 spawnPosition)
    {
        switch (GetTypeFromPixel(mapColor))
        {
            case ObjectType.FreeSpace:
                //Do nothing
                break;
            case ObjectType.Undestructable:
                GameObject g = Instantiate(unbreakablePrefab, spawnPosition, Quaternion.identity);
                g.transform.parent = transform;
                break;
            case ObjectType.Destructable:
                int randomRotation = Random.Range(0, 4);
                Vector3 randomRotationVector = Vector3.zero;
                switch (randomRotation)
                {
                    case 0: randomRotationVector = Vector3.zero; break;
                    case 1: randomRotationVector = Vector3.up * 90; break;
                    case 2: randomRotationVector = Vector3.up * 180; break;
                    case 3: randomRotationVector = Vector3.up * 270; break;
                }
                DestructableBase g2 = Instantiate(destructableData.Prefab, spawnPosition, Quaternion.Euler(randomRotationVector));
                g2.transform.parent = transform;
                g2.OnDeath += HandleDeathDestructable;
                break;
        }
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
}
public enum ObjectType
{
    FreeSpace,
    Undestructable,
    Destructable
}