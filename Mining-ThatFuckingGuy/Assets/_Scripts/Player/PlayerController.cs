using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cameraMain;
    [SerializeField] private LayerMask generalCollider;
    [SerializeField] private float speed;
    [Header("Gravity")]
    [SerializeField] private float gravityDecreaseMultiplier;
    [SerializeField] private float gravityVelocitySpeed;
    [SerializeField] private float gravityTerminalVelocity = -20f;
    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private AnimationCurve jumpCurve;
    [Header("Visual")]
    [SerializeField] private Transform visual;
    [SerializeField] private float timeAirTimer;
    [SerializeField] private Backpack backpack;

    public static DungeonManager CurrentDungeon;
    private const float SKIN = 0.01f;

    private InputManager inputM;
    private float verticalVelocity;
    private float horizontalVelocity;
    private CapsuleCollider c;
    private bool jumpState;
    private Vector3 targetJumpPosition;
    private Vector3 startJumpPosition;
    private float lastTimeGrounded;
    private PlayerAnimationController animationController;
    void Awake()
    {
        inputM = FindAnyObjectByType<InputManager>();
        c = GetComponent<CapsuleCollider>();
        animationController = GetComponent<PlayerAnimationController>();
    }


    void Update()
    {
        Vector2 input = inputM.MovementVectorNormalized();
        if (IsGrounded()) lastTimeGrounded = Time.time;
        HandleVertical();
        HandleHorizontal(input);
        HandleVisual(input);
    }

    private void HandleVisual(Vector2 input)
    {
        if (input.x > 0)
        {
            visual.localEulerAngles = Vector3.zero;
        }
        else if (input.x < 0)
        {
            visual.localEulerAngles = new Vector3(0, 180, 0);
        }
    }

    private void HandleVertical()
    {
        if (jumpState)
        {

            float remaining = targetJumpPosition.y - transform.position.y;
            if (remaining <= SKIN)
            {
                verticalVelocity = 0f;
                jumpState = false;
                return;
            }
            float normalized = (transform.position.y - startJumpPosition.y) / (targetJumpPosition.y - startJumpPosition.y);
            float jumpMultiplier = jumpCurve.Evaluate(normalized);
            float desired = Mathf.Min(verticalVelocity * Time.deltaTime * jumpMultiplier, remaining);
            float allowed = SweepMove(Vector3.up, desired, out bool blocked);
            transform.position += Vector3.up * allowed;
            Debug.Log("Jumping");
            if (blocked)
            {
                jumpState = false;
                verticalVelocity = 0f;
            }
            return;
        }

        Debug.Log("Falling");
        verticalVelocity += Physics.gravity.y / gravityDecreaseMultiplier * Time.deltaTime * gravityVelocitySpeed;
        verticalVelocity = Mathf.Max(verticalVelocity, gravityTerminalVelocity);

        float fallDesired = -verticalVelocity * Time.deltaTime;
        float fallAllowed = SweepMove(Vector3.down, fallDesired, out bool grounded);
        transform.position += Vector3.down * fallAllowed;
        if (grounded) verticalVelocity = 0;
    }

    private void HandleHorizontal(Vector2 input)
    {
        if (input.x == 0) return;

        Vector3 direction = input.x > 0 ? transform.forward : -transform.forward;
        float desired = Mathf.Abs(input.x) * speed * Time.deltaTime;

        float allowed = SweepMove(direction, desired, out _);
        horizontalVelocity = allowed;
        transform.position += direction * allowed * backpack.GetSpeedReduceMultiplier();
    }


    private float SweepMove(Vector3 direction, float desiredDistance, out bool blocked)
    {
        if (desiredDistance <= 0f) { blocked = false; return 0f; }

        float radius = c.radius;
        float halfHeight = Mathf.Max(c.height / 2f - radius, 0f);
        Vector3 center = transform.position + c.center;
        Vector3 point1 = center + Vector3.up * halfHeight;
        Vector3 point2 = center - Vector3.up * halfHeight;

        if (Physics.CapsuleCast(point1, point2, radius, direction, out RaycastHit hit, desiredDistance + SKIN, generalCollider))
        {
            blocked = true;
            return Mathf.Max(hit.distance - SKIN, 0f);
        }

        blocked = false;
        return desiredDistance;
    }
    public float GetAnyDirectionVelocity()
    {
        if (inputM.MovementVectorNormalized() == Vector2.zero) return 0;
        return 1;
    }
    private bool IsGrounded()
    {
        float radius = c.radius;
        float halfHeight = Mathf.Max(c.height / 2f - radius, 0f);
        Vector3 center = transform.position + c.center;
        Vector3 point1 = center + Vector3.up * halfHeight;
        Vector3 point2 = center - Vector3.up * halfHeight;
        return Physics.CapsuleCast(point1, point2, radius, Vector3.down, groundCheckDistance);
    }

    private float CalculateJumpPower()
    {
        float effectiveGravity = Mathf.Abs(Physics.gravity.y / gravityDecreaseMultiplier * gravityVelocitySpeed);
        return Mathf.Sqrt(2f * effectiveGravity * jumpHeight);
    }

    private void Jump()
    {
        if (IsGrounded() || Time.time - lastTimeGrounded <= timeAirTimer)
        {
            verticalVelocity = CalculateJumpPower();
            jumpState = true;
            targetJumpPosition = transform.position + Vector3.up * jumpHeight;
            startJumpPosition = transform.position;
        }
    }
    public Backpack GetBackpack()
    {
        return backpack;
    }
    public Transform GetVisual()
    {
        return visual;
    }
    public CinemachineCamera GetCamera()
    {
        return cameraMain;
    }
    public void DisableRequests()
    {
        animationController.BeforeDisable();
    }
    public void EmptyAllBackpackDirt(RobotInside robotStoraged)
    {
        if (backpack.CurrentDirtAmount() <= 0) return;

        List<ParticleBase> particlesCopy = new List<ParticleBase>(backpack.GetParticles());
        float averageDirtAmount = backpack.CurrentDirtAmount() / (float)backpack.GetParticles().Count;

        backpack.ResetDirtInStoraged();
        backpack.ResetParticles();
        StartCoroutine(EmptyDirtAnimation(particlesCopy, robotStoraged.transform, averageDirtAmount, robotStoraged.SetDirtStorage));
    }
    IEnumerator EmptyDirtAnimation(List<ParticleBase> particlesCopy, Transform robotStoraged, float avarageDirtAmount, Action<float, ParticleBase> Invoke)
    {
        foreach (var a in particlesCopy)
        {
            a.transform.parent = null;
            a.gameObject.SetActive(true);
            a.PlayAnimation(backpack.transform.position, robotStoraged.transform, avarageDirtAmount, Invoke);
            yield return new WaitForFixedUpdate();
        }
    }
    void OnEnable()
    {
        animationController.enabled = true;
        InputManager.OnJump += Jump;
    }
    void OnDisable()
    {
        InputManager.OnJump -= Jump;
    }
}