using UnityEngine;

public class RobotUseController : MonoBehaviour
{

    [SerializeField] private float getInDistance;
    private Collider[] resultsBuffer;
    private bool isRiding;
    private PlayerController player;
    private Robot robot;
    void Start()
    {
        player = GetComponent<PlayerController>();
        robot = FindAnyObjectByType<Robot>();
        robot.GetInRobot(player);
        isRiding = true;
    }
    private void UseRobot()
    {
        if (!isRiding)
        {
            float distance = Vector3.Distance(transform.position, robot.transform.position);
            if (distance < getInDistance)
            {
                isRiding = true;
                robot.GetInRobot(player);
            }
        }
        else if (isRiding)
        {
            robot.GetOutRobot(player);
            isRiding = false;
        }

    }
    void OnEnable()
    {
        InputManager.OnUse += UseRobot;
    }
    private void OnDisable()
    {
        InputManager.OnUse -= UseRobot;
    }

}
