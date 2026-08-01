using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNode : MonoBehaviour
{
    public event Action<UpgradeNode> OnUpgraded;
    [SerializeField] private NodeUpgradeSO data;
    [SerializeField] private TextMeshProUGUI nodeDisplayName;
    [SerializeField] private UpgradeNode prerequisite;

    Button button;
    void Awake()
    {
        button = GetComponent<Button>();
        nodeDisplayName.text = data.DisplayName;
    }
    public void Upgrade()
    {
        data.Apply(ToolController.Instance);
        OnUpgraded?.Invoke(this);
    }
    public void OpenSelf()
    {
        gameObject.SetActive(true);
    }
    public UpgradeNode GetPrerequisite()
    {
        return prerequisite;
    }
}
