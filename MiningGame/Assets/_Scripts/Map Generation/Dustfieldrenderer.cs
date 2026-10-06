using System.Collections.Generic;
using UnityEngine;

// Chunk basina tek mesh + vertex-renkli (per-corner) alpha ile calisiyor.
// Onceki versiyon her quad'in KENDI UV'sine gore feather yapiyordu, bu da
// bitisik iki "hala kapali" hucre arasinda bile kenar solmasina (gereksiz ic
// dikis) yol aciyordu. Bu versiyonda her kose (corner), o koseyi paylasan 4
// hucreden kacinin hala "hidden" oldugunu sayiyor ve alpha'yi buna gore
// belirliyor (4/4 = tam opak, komsu hucreler de hidden'sa asla solmuyor;
// sadece gercekten acik/revealed bir komsuya yakin koseler soluyor).
// Bitisik quad'lar ayni koseyi bagimsiz hesaplasa bile SONUC hep ayni
// (sadece izgara komsulugu fonksiyonu), yani dikis olmadan sorunsuz birlesiyor.
//
// DungeonManager tarafinda HICBIR SEY DEGISMIYOR — RegisterCell / HideCell /
// RebuildDirtyChunks ayni imzalarla calismaya devam ediyor.
public class DustFieldRenderer : MonoBehaviour
{
    [SerializeField] private Material dustMaterial;
    [SerializeField] private int chunkSize = 16;
    [SerializeField] private int subdivisions = 4;
    [SerializeField] private float fadeSharpness = 3f;
    private class Chunk
    {
        public Mesh mesh;
    }

    private readonly Dictionary<Vector2Int, Chunk> chunks = new();
    private readonly HashSet<Vector2Int> hiddenCells = new();               // hala dust'li (kapali) hucreler
    private readonly Dictionary<Vector2Int, Vector3> cellPositions = new(); // kayitli her hucrenin dunya pozisyonu
    private readonly HashSet<Vector2Int> dirtyChunkCoords = new();

    private static readonly List<Vector3> vertexBuffer = new();
    private static readonly List<int> triangleBuffer = new();
    private static readonly List<Color> colorBuffer = new();

    private Vector2Int ChunkCoord(int z, int y)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)z / chunkSize),
            Mathf.FloorToInt((float)y / chunkSize)
        );
    }

    public void RegisterCell(int z, int y, Vector3 worldPos)
    {
        Vector2Int cell = new Vector2Int(z, y);
        cellPositions[cell] = worldPos;
        hiddenCells.Add(cell);
        MarkCellAndNeighborsDirty(cell);
    }

    // Isim eskisinden kaliyor ama efekti "reveal" — dust'i kaldirir.
    public void HideCell(int z, int y)
    {
        Vector2Int cell = new Vector2Int(z, y);
        if (hiddenCells.Remove(cell))
            MarkCellAndNeighborsDirty(cell);
    }

    // Degisen hucrenin kendi chunk'i YETMEZ: komsu hucrelerin kose alpha'si
    // da degisebilir, ve o komsu baska bir chunk'ta olabilir. O yuzden
    // 3x3'luk komsuluktaki her chunk koordinatini dirty isaretliyoruz.
    // NOT: bu, o komsu chunk'ta GERCEKTEN hucre oldugu anlamina gelmez —
    // asil kontrol RebuildDirtyChunks'ta yapiliyor (bkz. ChunkHasAnyHiddenCell).
    private void MarkCellAndNeighborsDirty(Vector2Int cell)
    {
        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                dirtyChunkCoords.Add(ChunkCoord(cell.x + dz, cell.y + dy));
            }
        }
    }

    private bool IsHidden(int z, int y)
    {
        return hiddenCells.Contains(new Vector2Int(z, y));
    }

    public void RebuildDirtyChunks()
    {
        foreach (Vector2Int coord in dirtyChunkCoords)
        {
            bool chunkExists = chunks.TryGetValue(coord, out Chunk chunk);

            if (!chunkExists)
            {
                // BUG FIX: eskiden burada kosulsuz CreateChunk cagriliyordu.
                // Chunk sinirindaki (ve dungeon'in en dis kenarindaki) her hucre
                // komsusunun chunk'ini da dirty isaretledigi icin, o chunk'ta
                // hicbir zaman tek bir hidden hucre olmasa bile bombos bir
                // GameObject+Mesh yaratiliyordu. Simdi once gercekten en az
                // bir hidden hucresi var mi diye bakiyoruz; yoksa hic yaratma.
                if (!ChunkHasAnyHiddenCell(coord)) continue;
                chunk = CreateChunk(coord);
                chunks[coord] = chunk;
            }

            RebuildChunkMesh(coord, chunk);
        }
        dirtyChunkCoords.Clear();
    }

    private bool ChunkHasAnyHiddenCell(Vector2Int coord)
    {
        int zStart = coord.x * chunkSize;
        int yStart = coord.y * chunkSize;

        for (int z = zStart; z < zStart + chunkSize; z++)
        {
            for (int y = yStart; y < yStart + chunkSize; y++)
            {
                if (IsHidden(z, y)) return true;
            }
        }
        return false;
    }

    private Chunk CreateChunk(Vector2Int coord)
    {
        GameObject go = new GameObject($"DustChunk_{coord.x}_{coord.y}");
        go.transform.parent = transform;

        Chunk chunk = new Chunk
        {
            mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 }
        };
        go.AddComponent<MeshFilter>().mesh = chunk.mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = dustMaterial;
        return chunk;
    }

    private void RebuildChunkMesh(Vector2Int coord, Chunk chunk)
    {
        vertexBuffer.Clear();
        triangleBuffer.Clear();
        colorBuffer.Clear();

        int zStart = coord.x * chunkSize;
        int yStart = coord.y * chunkSize;
        int res = subdivisions + 1; // her eksende vertex satırı sayısı

        for (int z = zStart; z < zStart + chunkSize; z++)
        {
            for (int y = yStart; y < yStart + chunkSize; y++)
            {
                if (!IsHidden(z, y)) continue;

                Vector3 pos = cellPositions[new Vector2Int(z, y)];

                // Dört gerçek köşe alpha'sı - eskisiyle AYNI hesap
                float a00 = CornerColor(z, y, -1, -1).a; // sol-alt
                float a10 = CornerColor(z, y, 1, -1).a;  // sağ-alt
                float a11 = CornerColor(z, y, 1, 1).a;   // sağ-üst
                float a01 = CornerColor(z, y, -1, 1).a;  // sol-üst

                int baseIndex = vertexBuffer.Count;

                for (int iy = 0; iy < res; iy++)
                {
                    float v = iy / (float)subdivisions;
                    for (int iz = 0; iz < res; iz++)
                    {
                        float u = iz / (float)subdivisions;

                        float localZ = Mathf.Lerp(-0.5f, 0.5f, u);
                        float localY = Mathf.Lerp(-0.5f, 0.5f, v);
                        vertexBuffer.Add(pos + new Vector3(0f, localY, localZ));

                        // Gerçek bilinear interpolasyon
                        float bottom = Mathf.Lerp(a00, a10, u);
                        float top = Mathf.Lerp(a01, a11, u);
                        float alpha = Mathf.Lerp(bottom, top, v);

                        // Doğrusal geçişi, iç kısım uzun süre opak kalan bir eğriye çeviriyoruz
                        alpha = 1f - Mathf.Pow(1f - alpha, fadeSharpness);

                        colorBuffer.Add(new Color(0f, 0f, 0f, alpha));
                    }
                }

                for (int iy = 0; iy < subdivisions; iy++)
                {
                    for (int iz = 0; iz < subdivisions; iz++)
                    {
                        int i00 = baseIndex + iy * res + iz;
                        int i10 = i00 + 1;
                        int i01 = i00 + res;
                        int i11 = i01 + 1;

                        triangleBuffer.Add(i00); triangleBuffer.Add(i11); triangleBuffer.Add(i10);
                        triangleBuffer.Add(i00); triangleBuffer.Add(i01); triangleBuffer.Add(i11);
                    }
                }
            }
        }

        chunk.mesh.Clear();
        chunk.mesh.SetVertices(vertexBuffer);
        chunk.mesh.SetColors(colorBuffer);
        chunk.mesh.SetTriangles(triangleBuffer, 0);
        chunk.mesh.RecalculateBounds();
    }
    // cellZ,cellY = quad'in ait oldugu hucre; dz,dy = hangi koseye baktigimiz (-1/+1)
    private Color CornerColor(int cellZ, int cellY, int dz, int dy)
    {
        int hiddenCount = 0;
        if (IsHidden(cellZ, cellY)) hiddenCount++;             // kendisi (zaten hep true)
        if (IsHidden(cellZ + dz, cellY)) hiddenCount++;        // yatay komsu
        if (IsHidden(cellZ, cellY + dy)) hiddenCount++;        // dikey komsu
        if (IsHidden(cellZ + dz, cellY + dy)) hiddenCount++;   // capraz komsu

        float alpha = hiddenCount / 4f;
        return new Color(0f, 0f, 0f, alpha);
    }
}