using System;
using UnityEngine;

public class Robot : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform rideTransform;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private RobotInside inside;



    private InputManager input;
    private DungeonGate currentGate;
    private PlayerController currentPlayer;
    private bool isEnteredToGate;
    void Awake()
    {
        input = FindAnyObjectByType<InputManager>();
        inside.OnPlayerEnterState += PlayerRobotEnterState;
    }
    void Update()
    {
        Vector2 inputVector = input.MovementVectorNormalized();
        if (inputVector == Vector2.zero) return;

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
    private void PlayerRobotEnterState(bool obj,PlayerController player)
    {
        if (obj)
        {
            player.EmptyAllBackpackDirt(inside);
        }
    }
    public void GetInRobot(PlayerController user)
    {
        currentPlayer = user;
        this.enabled = true;
        user.DisableRequests();
        user.GetCamera().Target.TrackingTarget = cameraPosition;
        user.enabled = false;
        user.transform.parent = rideTransform;
        user.GetVisual().forward = rideTransform.forward;

        user.transform.localPosition = Vector3.zero;
    }
    public void GetOutRobot(PlayerController user)
    {
        currentPlayer = null;
        this.enabled = false;
        user.enabled = true;
        user.GetCamera().Target.TrackingTarget = user.transform;
        user.transform.parent = null;
    }
    private void TryEnterToGate()
    {
        if (currentPlayer == null) return;

        if (!isEnteredToGate)
        {
            if (currentGate == null) return;

            currentGate.AcceptRobot(this);
            GetOutRobot(currentPlayer);
        }
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
