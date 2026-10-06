using System;
using Unity.Cinemachine;
using UnityEngine;

public class Robot : MonoBehaviour
{
    public static event Action<bool> OnGateEnterState;
    [SerializeField] private float speed;
    [SerializeField] private Transform rideTransform;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private RobotInside inside;
    [SerializeField] private float fuelUsePerSecond;
    [SerializeField] private GameObject dropUI;



    private InputManager input;
    private DungeonGate currentGate;
    private PlayerController currentPlayer;
    private bool isEnteredToGate;
    private float lastTimeEnteredToGate;
    void Start()
    {
        input = FindAnyObjectByType<InputManager>();
        inside.OnPlayerEnterState += PlayerRobotEnterState;
    }
    void Update()
    {
        if (isEnteredToGate) return;

        Vector2 inputVector = input.MovementVectorNormalized();
        if (inputVector == Vector2.zero || !inside.IsFull()) return;
        inside.UseFuel(Time.deltaTime * fuelUsePerSecond);

        transform.position += Vector3.up * inputVector.y * Time.deltaTime * speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out DungeonGate gate))
        {
            currentGate = gate;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out DungeonGate gate))
        {
            currentGate = null;
        }

    }
    private void PlayerRobotEnterState(bool obj, PlayerController player)
    {
        if (obj)
        {
            player.EmptyAllBackpackDirt(inside);
            player.EmptyAllDropsOnMiningTool();
        }
    }
    public void GetInRobot(PlayerController user)
    {
        OnGateEnterState?.Invoke(true);
        dropUI.SetActive(true);
        currentPlayer = user;
        this.enabled = true;
        user.DisableRequests();
        user.enabled = false;
        user.transform.parent = rideTransform;
        user.GetVisual().forward = rideTransform.forward;

        user.transform.localPosition = Vector3.zero;
    }
    public void GetOutRobot(PlayerController user, out bool success)
    {
        success = true;
        if (!isEnteredToGate || lastTimeEnteredToGate + 0.5f > Time.time)
        {
            success = false;
            return;
        }
        dropUI.SetActive(false);
        currentPlayer = null;
        this.enabled = false;
        user.enabled = true;
        user.GetCamera().Target.TrackingTarget = user.transform;
        user.transform.parent = null;
        OnGateEnterState?.Invoke(false);

    }
    private void TryEnterToGate()
    {
        if (currentPlayer == null || currentGate == null) return;

        if (!isEnteredToGate)
        {
            currentGate.AcceptRobot(this, out bool success);
            if (success)
            {
                currentPlayer.GetCamera().GetComponent<CinemachineFollow>().FollowOffset.z = 0;
                isEnteredToGate = true;
                lastTimeEnteredToGate = Time.time;
            }
        }
        else
        {
            GetOutGateSetup();
            currentGate.RemoveRobot();
            isEnteredToGate = false;
            currentGate = null;
        }

    }
    public void GetOutGateSetup()
    {
        currentPlayer.GetCamera().GetComponent<CinemachineFollow>().FollowOffset.z = -12f;
        currentPlayer.GetCamera().Target.TrackingTarget = cameraPosition;
        dropUI.SetActive(false);
    }
    public PlayerController GetCurrentPlayer()
    {
        return currentPlayer;
    }
    public Transform CameraPosition()
    {
        return cameraPosition;
    }
    private void OnEnable()
    {
        InputManager.OnInteract += TryEnterToGate;
    }

    private void OnDisable()
    {
        InputManager.OnInteract -= TryEnterToGate;
    }
}
