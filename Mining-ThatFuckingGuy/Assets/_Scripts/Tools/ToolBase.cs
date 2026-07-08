using System.Collections.Generic;
using UnityEngine;

public abstract class ToolBase : MonoBehaviour
{
    public ToolSO Data;
    public bool MainUseState { get; set; }
    public Dictionary<UpgradeType, float> stats = new();

    public bool AlternativeState { get; set; }
    public abstract void UpdateUse();
    public abstract void UpgradeSelf(UpgradeData upgradeData);


    public virtual void MainUse(bool state)
    {
        MainUseState = state;
    }
    public virtual void AlternativeUse(bool state)
    {
        AlternativeState = state;
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
    public void OnEnable()
    {
        InputManager.OnMouseLeft += MainUse;
        InputManager.OnMouseRight += AlternativeUse;
    }
    public void OnDisable()
    {
        MainUseState = false;
        AlternativeState = false;
        InputManager.OnMouseLeft -= MainUse;
        InputManager.OnMouseRight -= AlternativeUse;
    }

}
