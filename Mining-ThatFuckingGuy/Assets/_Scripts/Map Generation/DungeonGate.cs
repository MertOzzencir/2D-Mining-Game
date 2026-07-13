using UnityEngine;

public class DungeonGate : MonoBehaviour
{
    private DungeonManager owner;
    private Robot currentRobot;
    void Awake()
    {
        owner = transform.parent.GetComponent<DungeonManager>();
        transform.position = owner.transform.position + new Vector3(0, owner.DungeonHeight() / 2 - 2, 0);
    }
    public void AcceptRobot(Robot robot)
    {
        if (currentRobot == null)
        {
            robot.transform.position = transform.position;
            PlayerController.CurrentDungeon = owner;
        }
    }
}
