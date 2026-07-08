using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNode : MonoBehaviour
{
    [SerializeField] private ToolBase tool;
    [SerializeField] private UpgradeData[] upgradeDatas;
    [SerializeField] private Button[] buttons;
    void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int capturexIndex = i;
            buttons[i].onClick.AddListener(() => Upgrade(capturexIndex));
            buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = upgradeDatas[i].UpgradeName;
        }
    }
    private void Set(UpgradeData data)
    {
        tool.UpgradeSelf(data);
    }
    [ContextMenu("Upgrade")]
    public void Upgrade(int index)
    {
        Debug.Log(index);
        if (index < upgradeDatas.Length)
            Set(upgradeDatas[index]);
    }
}
[Serializable]
public enum UpgradeType
{
    ToolDamage,
    ToolCooldown,
    ToolMaxRange
}
[Serializable]
public class UpgradeData
{
    public UpgradeType Type;
    public float Amount;
    public string UpgradeName;
}
