using System;
using System.Collections;
using UnityEngine;

public class DungeonGate : MonoBehaviour
{
    public event Action OnPlayerHasEntered;
    [SerializeField] private float animationTimer = 5f;
    private DungeonManager owner;
    private Robot currentRobot;
    public void AcceptRobot(Robot robot, out bool success)
    {
        success = false;
        if (currentRobot == null)
        {
            success = true;
            StartCoroutine(GateEnterAnimation(robot));
            PlayerController.CurrentDungeon = owner;
            OnPlayerHasEntered?.Invoke();
        }
    }
    public void RemoveRobot()
    {
        StopAllCoroutines();
        currentRobot = null;
        PlayerController.CurrentDungeon = null;
    }
    IEnumerator GateEnterAnimation(Robot robot)
    {
        while (Mathf.Abs(robot.transform.position.y - transform.position.y) > 0.01f)
        {
            robot.transform.position = Vector3.Lerp(robot.transform.position, new Vector3(robot.transform.position.x, transform.position.y, robot.transform.position.z), animationTimer * Time.deltaTime);
            yield return null;
        }
        robot.transform.position = new Vector3(robot.transform.position.x, transform.position.y, robot.transform.position.z);
        //robot.GetOutRobot(robot.GetCurrentPlayer());
    }
    public void SetGate(DungeonManager owner)
    {
        this.owner = owner;
        transform.position = owner.transform.position + new Vector3(0, owner.DungeonHeight() / 2 - 1, 0);

    }
}
