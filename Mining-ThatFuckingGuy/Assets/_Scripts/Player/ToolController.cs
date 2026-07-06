using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ToolController : MonoBehaviour
{

    [SerializeField] private float attackDamage;
    [SerializeField] private Transform toolRotation;
    [SerializeField] private Transform toolPlace;
    [SerializeField] private ToolBase[] tools;

    private ToolBase CurrentTool;
    void Update()
    {
        if (CurrentTool == null) return;

        CurrentTool.UpdateUse();

        Plane plane = new Plane(Vector3.right, CurrentTool.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            hitPoint.x = 0;
            Vector3 direction = (hitPoint - toolRotation.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            toolRotation.rotation = lookRotation;
        }
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
        CurrentTool.Equip(toolPlace.transform);
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
