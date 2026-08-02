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
    private int currentLevel;
    private int maxLevel;
    void Awake()
    {
        button = GetComponent<Button>();
        maxLevel = data.MaxLevel;
        nodeDisplayName.text = data.DisplayName;
    }
    public void Upgrade()
    {
        if (currentLevel >= maxLevel) return;

        data.Apply(ToolController.Instance, currentLevel);
        NextLevel();
        OnUpgraded?.Invoke(this);
    }
    public void OpenSelf()
    {
        gameObject.SetActive(true);
    }
    public void CloseSelf()
    {
        gameObject.SetActive(false);
    }
    public UpgradeNode GetPrerequisite()
    {
        return prerequisite;
    }
    public void NextLevel()
    {
        currentLevel++;
        if (currentLevel >= maxLevel) CloseSelf();
    }
}
