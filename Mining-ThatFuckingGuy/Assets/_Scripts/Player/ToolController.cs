using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ToolController : MonoBehaviour
{

    [SerializeField] private float attackDamage;
    [SerializeField] private Transform toolRotation;
    [SerializeField] private ToolBase[] tools;

    private ToolBase CurrentTool;
    void Update()
    {
        if (CurrentTool == null) return;

        CurrentTool.UpdateUse();
    }
    private void PickTool(int obj)
    {
        if (obj > tools.Length)
        {
            if (CurrentTool != null)
            {
                CurrentTool.DeEquip();
                CurrentTool = null;
            }
            return;
        }

        obj -= 1;
        if (CurrentTool != null)
        {
            CurrentTool.DeEquip();
            if (CurrentTool == tools[obj])
            {
                CurrentTool = null;
                return;
            }
        }

        CurrentTool = tools[obj];
        CurrentTool.Equip(toolRotation.transform);
    }
    void OnEnable()
    {
        InputManager.OnNumbers += PickTool;
    }

    void OnDisable()
    {
        InputManager.OnNumbers -= PickTool;
    }
}
