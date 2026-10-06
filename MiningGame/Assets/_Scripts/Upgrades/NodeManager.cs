using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    [SerializeField] private UpgradeNode[] allNodes;

    void Awake()
    {
        Robot.OnGateEnterState += ActiveSelf;

        allNodes = GetComponentsInChildren<UpgradeNode>(true);
        foreach (var a in allNodes)
        {
            a.OnUpgraded += Unlock;
            if (a.GetPrerequisite() == null) a.OpenSelf();
        }
    }
    public void Unlock(UpgradeNode upgradedNode)
    {
        upgradedNode.OnUpgraded -= Unlock;
        foreach (var a in allNodes)
        {
            if (a.GetPrerequisite() == upgradedNode)
            {
                Debug.Log("Found one: " + a.name);

                a.OpenSelf();
            }
        }
    }
    private void ActiveSelf(bool obj)
    {
        gameObject.SetActive(obj);
    }
}
