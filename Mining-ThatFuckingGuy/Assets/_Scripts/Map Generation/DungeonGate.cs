using System.Collections;
using UnityEngine;

public class DungeonGate : MonoBehaviour
{
    [SerializeField] private float animationTimer = 5f;
    private DungeonManager owner;
    private Robot currentRobot;
    void Awake()
    {
        owner = transform.parent.GetComponent<DungeonManager>();
        transform.position = owner.transform.position + new Vector3(0, owner.DungeonHeight() / 2 - 2, 0);
    }
    public void AcceptRobot(Robot robot,out bool success)
    {
        success = false;
        if (currentRobot == null)
        {
            success = true;
            StartCoroutine(GateEnterAnimation(robot));
            PlayerController.CurrentDungeon = owner;
        }
    }
    IEnumerator GateEnterAnimation(Robot robot)
    {
        while (Mathf.Abs(robot.transform.position.y - transform.position.y) > 0.01f)
        {
            robot.transform.position = Vector3.Lerp(robot.transform.position, new Vector3(robot.transform.position.x, transform.position.y, robot.transform.position.z), animationTimer * Time.deltaTime);
            yield return null;
        }
        robot.transform.position = new Vector3(robot.transform.position.x, transform.position.y, robot.transform.position.z);
        robot.GetOutRobot(robot.GetCurrentPlayer());
    }
}
