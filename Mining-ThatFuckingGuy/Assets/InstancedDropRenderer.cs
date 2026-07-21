using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InstancedDropRenderer : MonoBehaviour
{
    [SerializeField] private DropType type;
    [SerializeField] private int index;
    private Dictionary<DropType, DropBatch> batches = new();

    private InstanceData current;
    private DungeonManager dungeonManager;
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
        return index;
    }
    float timer;
    void Update()
    {
        timer += Time.deltaTime;
        foreach (var batch in batches.Values)
        {
            if (batch.Instances.Count > 0)
            {
                for (int i = 0; i < batch.Instances.Count; i++)
                {
                    current = batch.Instances[i];
                    float yoffSet = Mathf.Sin(timer) / 4;
                    current.objectToWorld.m13 = yoffSet + batch.BaseY[i];
                    batch.Instances[i] = current;
                }

                Graphics.RenderMeshInstanced(batch.RenderParams, batch.Mesh, 0, batch.Instances);
            }
        }
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
        inst.objectToWorld = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.zero); // scale=0, tamamen görünmez
        batch.Instances[index] = inst;
    }
    [ContextMenu("Remove Drop Debug")]
    public void RemoveDropDebug()
    {
        RemoveDrop(type, index);
    }
}

public class DropBatch
{
    public RenderParams RenderParams;
    public Mesh Mesh;
    public List<InstanceData> Instances = new();
    public List<float> BaseY = new();
}
public struct InstanceData
{
    public Matrix4x4 objectToWorld;
    public uint renderingLayerMask;
}