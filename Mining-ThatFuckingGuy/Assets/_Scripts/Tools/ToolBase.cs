using System.Collections.Generic;
using UnityEngine;

public abstract class ToolBase : MonoBehaviour
{
    public Transform VisualTransform;
    public Transform AimPositionTransform;
    public ToolSO Data;
    public bool MainUseState { get; set; }
    public Dictionary<UpgradeType, float> stats = new();

    public bool AlternativeState { get; set; }
    public virtual void Awake()
    {

    }
    public virtual void UpdateUse()
    {
        HandleRotation(VisualTransform);
        HandleRotation(AimPositionTransform);
    }
    public abstract void UpgradeSelf(UpgradeData upgradeData);


    public virtual void MainUse(bool state)
    {
        MainUseState = state;
    }
    public virtual void AlternativeUse(bool state)
    {
        AlternativeState = state;
    }
    public virtual void InteractUse()
    {

    }
    public virtual void Equip(Transform newP)
    {
        gameObject.SetActive(true);
        transform.parent = newP;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }
    public virtual void DeEquip()
    {
        gameObject.SetActive(false);
        transform.parent = null;
    }
    public void HandleRotation(Transform t)
    {
        Plane plane = new Plane(Vector3.right, t.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = (hitPoint - t.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            t.rotation = lookRotation;
        }
    }
    public virtual void OnEnable()
    {
        InputManager.OnMouseLeft += MainUse;
        InputManager.OnMouseRight += AlternativeUse;
        InputManager.OnInteract += InteractUse;
    }
    public virtual void OnDisable()
    {
        MainUseState = false;
        AlternativeState = false;
        InputManager.OnMouseLeft -= MainUse;
        InputManager.OnMouseRight -= AlternativeUse;
        InputManager.OnInteract -= InteractUse;
    }

}
