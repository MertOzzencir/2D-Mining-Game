using System.Collections.Generic;
using UnityEngine;

public abstract class ToolBase : MonoBehaviour
{
    public Transform VisualTransform;
    public Transform AimPositionTransform;
    public PlayerController Player { get; set; }
    public ToolSO Data;
    public bool MainUseState { get; set; }

    public Dictionary<UpgradeType, float> Stats = new Dictionary<UpgradeType, float>();
    public bool AlternativeState { get; set; }
    public virtual void Awake()
    {
        Player = FindAnyObjectByType<PlayerController>();

    }
    public virtual void UpdateUse()
    {
        HandleRotation(VisualTransform);
        HandleRotation(AimPositionTransform);
    }

    public virtual void SetStats()
    {
        Stats[UpgradeType.Cooldown] = Data.CooldownTimer;
        Stats[UpgradeType.Range] = Data.Range;
    }
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
        InputManager.OnRotate += InteractUse;
    }
    public virtual void OnDisable()
    {
        MainUseState = false;
        AlternativeState = false;
        InputManager.OnMouseLeft -= MainUse;
        InputManager.OnMouseRight -= AlternativeUse;
        InputManager.OnRotate -= InteractUse;
    }
    private int cooldownApplyCount = 0;
    public virtual void UpgradeSelf(UpgradeType type, float amount)
    {
        switch (type)
        {
            case UpgradeType.Cooldown:
                cooldownApplyCount++;
                Debug.Log($"[{cooldownApplyCount}. çağrı] amount={amount}, ÖNCE={Stats[UpgradeType.Cooldown]}");
                Stats[UpgradeType.Cooldown] -= amount;
                Debug.Log($"[{cooldownApplyCount}. çağrı] SONRA={Stats[UpgradeType.Cooldown]}");
                break;
            case UpgradeType.Range:
                Stats[UpgradeType.Range] += amount;
                break;
        }
    }
    [ContextMenu("Debug Stats")]
    public void DebugStats()
    {
        foreach (var a in Stats)
        {
            Debug.Log("Current Stat: " + a.Key + " " + "Current Value: " + a.Value);
        }
    }
}

public enum UpgradeType
{
    Cooldown,
    Damage,
    Range,
    StorageLimit
}
