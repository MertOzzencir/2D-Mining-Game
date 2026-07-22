using System.Collections.Generic;
using UnityEngine;

public class InstancedDropRenderer : MonoBehaviour
{
    [SerializeField] private float fallLerpSpeed = 5f;

    private Dictionary<DropType, DropBatch> batches = new();
    private DungeonManager dungeonManager;
    private float timer;

    void Awake()
    {
        dungeonManager = GetComponent<DungeonManager>();
    }

    public int RegisterDrop(DropSO data, Material material, Vector3 position)
    {
        if (!batches.TryGetValue(data.DropType, out DropBatch batch))
        {
            material.enableInstancing = true;
            batch = new DropBatch
            {
                Mesh = data.Mesh,
                RenderParams = new RenderParams(material)
            };
            batches[data.DropType] = batch;
        }

        int index = batch.Instances.Count;
        batch.Instances.Add(new InstanceData
        {
            objectToWorld = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one),
            renderingLayerMask = 1u
        });
        batch.BaseY.Add(position.y);
        batch.Grounded.Add(false);
        batch.HasTarget.Add(false);
        batch.TargetY.Add(position.y);
        batch.PendingBlock.Add(null);

        return index;
    }

    void Update()
    {
        timer += Time.deltaTime;

        foreach (var kvp in batches)
        {
            DropType type = kvp.Key;
            DropBatch batch = kvp.Value;
            if (batch.Instances.Count == 0) continue;

            for (int i = 0; i < batch.Instances.Count; i++)
            {
                InstanceData inst = batch.Instances[i];

                if (!batch.Grounded[i])
                {
                    if (!batch.HasTarget[i])
                    {
                        FindTargetForDrop(batch, i);
                    }

                    if (batch.HasTarget[i])
                    {
                        float currentY = inst.objectToWorld.m13;
                        float newY = Mathf.Lerp(currentY, batch.TargetY[i], fallLerpSpeed * Time.deltaTime);
                        inst.objectToWorld.m13 = newY;
                        batch.Instances[i] = inst;

                        if (Mathf.Abs(newY - batch.TargetY[i]) < 0.01f)
                        {
                            SettleDrop(batch, type, i);
                        }
                    }
                }
                else
                {
                    InstanceData grounded = batch.Instances[i];
                    float bob = Mathf.Sin(timer) / 4f;
                    grounded.objectToWorld.m13 = batch.BaseY[i] + bob;
                    batch.Instances[i] = grounded;
                }
            }

            Graphics.RenderMeshInstanced(batch.RenderParams, batch.Mesh, 0, batch.Instances);
        }
    }
    public void UngroundDrop(DropType type, int index)
    {
        if (!batches.TryGetValue(type, out DropBatch batch)) return;
        if (index < 0 || index >= batch.Instances.Count) return;

        batch.Grounded[index] = false;
        batch.HasTarget[index] = false; // bir sonraki Update'te yeni hedef aranacak
    }
    private void FindTargetForDrop(DropBatch batch, int i)
    {
        InstanceData inst = batch.Instances[i];
        Vector3 stablePos = new Vector3(inst.objectToWorld.m03, batch.BaseY[i], inst.objectToWorld.m23);

        BlockData currentBlock = dungeonManager.GetBlockFromWorldPosition(stablePos, out _);

        BlockData lastEmpty = currentBlock;
        BlockData next = dungeonManager.GetEmptyBlockFromVertical(lastEmpty);

        while (next != null && next.IsEmpty)
        {
            lastEmpty = next;
            next = dungeonManager.GetEmptyBlockFromVertical(lastEmpty);
        }

        if (next == null)
        {
            return;
        }

        batch.TargetY[i] = lastEmpty.WorldPosition.y;
        batch.PendingBlock[i] = lastEmpty;
        batch.HasTarget[i] = true;
    }

    private void SettleDrop(DropBatch batch, DropType type, int i)
    {
        BlockData restBlock = batch.PendingBlock[i];
        Vector3 restPosition = restBlock.WorldPosition;

        InstanceData inst = batch.Instances[i];
        inst.objectToWorld.m03 = restPosition.x;
        inst.objectToWorld.m13 = restPosition.y;
        inst.objectToWorld.m23 = restPosition.z;
        batch.Instances[i] = inst;

        batch.BaseY[i] = restPosition.y;
        batch.Grounded[i] = true;

        restBlock.DropsOnBlock.Add(new DropReference(type, i));
    }

    public Vector3 GetDropPosition(DropType type, int index)
    {
        if (!batches.TryGetValue(type, out DropBatch batch)) return Vector3.zero;
        return batch.Instances[index].objectToWorld.GetPosition();
    }

    public bool TryGetMeshAndMaterial(DropType type, out Mesh mesh, out Material material)
    {
        if (batches.TryGetValue(type, out DropBatch batch))
        {
            mesh = batch.Mesh;
            material = batch.RenderParams.material;
            return true;
        }
        mesh = null;
        material = null;
        return false;
    }

    public void RemoveDrop(DropType type, int index)
    {
        if (!batches.TryGetValue(type, out DropBatch batch)) return;
        if (index < 0 || index >= batch.Instances.Count) return;

        InstanceData inst = batch.Instances[index];
        inst.objectToWorld = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.zero); // scale=0, görünmez
        batch.Instances[index] = inst;
    }
}

public class DropBatch
{
    public RenderParams RenderParams;
    public Mesh Mesh;
    public List<InstanceData> Instances = new();
    public List<float> BaseY = new();
    public List<bool> Grounded = new();

    public List<bool> HasTarget = new();
    public List<float> TargetY = new();
    public List<BlockData> PendingBlock = new();
}

public struct InstanceData
{
    public Matrix4x4 objectToWorld;
    public uint renderingLayerMask;
}