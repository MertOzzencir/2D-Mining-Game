using UnityEngine;

public class DungeonGate : MonoBehaviour
{
    private DungeonManager owner;
    void Awake()
    {
        owner = transform.parent.GetComponent<DungeonManager>();
        transform.position = owner.transform.position + new Vector3(0, owner.DungeonHeight() / 2 - 0.5f, -2);
    }
}
