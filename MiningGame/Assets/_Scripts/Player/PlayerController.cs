using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    [SerializeField] private int particlesPerFrame = 5;
    [SerializeField] private float dirtPerParticle = 10f;
    [SerializeField] private int emptyParticlePrewarmCountPerType = 4;
    [SerializeField] private CinemachineCamera cameraMain;
    [SerializeField] private LayerMask generalCollider;
    [SerializeField] private float speed;
    [SerializeField] private ParticleSystem movementParticle;
    [SerializeField] private ParticleSystem groundedParticle;
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

    public static DungeonManager CurrentDungeon { get; set; }
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
    private ToolController toolController;
    private InventoryController inventoryController;
    private HealthController healthController;
    private Dictionary<ParticleBase, GenericObjectPool<ParticleBase>> emptyParticlePools = new();
    private Dictionary<DestructableSO, float> collectedDirtByType = new();

    void Awake()
    {
        inputM = FindAnyObjectByType<InputManager>();
        c = GetComponent<CapsuleCollider>();
        animationController = GetComponent<PlayerAnimationController>();
        toolController = GetComponent<ToolController>();
        inventoryController = GetComponent<InventoryController>();
        healthController = GetComponent<HealthController>();
    }

    private bool wasGrounded = true;

    void Update()
    {
        Vector2 input = inputM.MovementVectorNormalized();
        bool grounded = IsGrounded();

        if (grounded && !jumpState)
        {
            animationController.SetBool("jump", false);
            lastTimeGrounded = Time.time;

            if (!wasGrounded)
            {

                groundedParticle.gameObject.SetActive(true);
                groundedParticle.Play();
            }
        }

        wasGrounded = grounded;

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
            if (blocked)
            {
                jumpState = false;
                verticalVelocity = 0f;
            }
            return;
        }

        verticalVelocity += Physics.gravity.y / gravityDecreaseMultiplier * Time.deltaTime * gravityVelocitySpeed;
        verticalVelocity = Mathf.Max(verticalVelocity, gravityTerminalVelocity);

        float fallDesired = -verticalVelocity * Time.deltaTime;
        float fallAllowed = SweepMove(Vector3.down, fallDesired, out bool grounded);
        transform.position += Vector3.down * fallAllowed;
        if (grounded) verticalVelocity = 0;
    }
    private bool wasMoving = false;
    private void HandleHorizontal(Vector2 input)
    {
        bool isMoving = input.x != 0;
        bool grounded = IsGrounded();
        bool shouldPlayParticle = isMoving && grounded;

        if (shouldPlayParticle != wasMoving)
        {
            if (shouldPlayParticle)
                movementParticle.Play();
            else
                movementParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            wasMoving = shouldPlayParticle;
        }

        if (!isMoving) return;

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
        return inputM.MovementVectorNormalized().x == 0 ? 0 : 1;
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
            animationController.SetBool("jump", true);
            movementParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            wasMoving = false;
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
 
    public HealthController GetHealthController()
    {
        return healthController;
    }
    public void DisableRequests()
    {
        animationController.BeforeDisable();
        toolController.enabled = false;
    }


    private GenericObjectPool<ParticleBase> GetOrCreateEmptyParticlePool(ParticleBase prefab)
    {
        if (!emptyParticlePools.TryGetValue(prefab, out var pool))
        {
            pool = new GenericObjectPool<ParticleBase>(
                factory: () => Instantiate(prefab, transform),
                prewarmCount: emptyParticlePrewarmCountPerType
            );
            emptyParticlePools[prefab] = pool;
        }
        return pool;
    }

    // DungeonManager, bir dirt particle backpack'e ULAŞTIĞINDA bunu çağırıyor
    public void AddCollectedDirt(DestructableSO data, float amount)
    {
        if (collectedDirtByType.ContainsKey(data))
            collectedDirtByType[data] += amount;
        else
            collectedDirtByType[data] = amount;
    }
    public void EmptyAllBackpackDirt(RobotInside robotStoraged)
    {
        float totalDirt = backpack.CurrentDirtAmount();
        if (totalDirt <= 0) return;

        List<KeyValuePair<DestructableSO, float>> breakdown = new(collectedDirtByType);

        backpack.ResetDirtInStoraged();
        collectedDirtByType.Clear();

        StartCoroutine(EmptyDirtAnimation(breakdown, robotStoraged.transform, robotStoraged.SetDirtStorage));
    }
    public void EmptyAllDropsOnMiningTool()
    {
        MiningTool miningTool = toolController.GetMiningTool();
        foreach (var a in miningTool.DropsOnTool())
        {
            inventoryController.AddDrop(a.Key, a.Value);
        }
        miningTool.ResetStorage();
    }

    private IEnumerator EmptyDirtAnimation(List<KeyValuePair<DestructableSO, float>> breakdown, Transform robotStoraged, Action<float> onArrive)
    {
        int spawned = 0;

        foreach (var kvp in breakdown)
        {
            DestructableSO data = kvp.Key;
            float amount = kvp.Value;

            GenericObjectPool<ParticleBase> pool = GetOrCreateEmptyParticlePool(data.DirtParticleVFX);

            int particleCount = Mathf.Max(1, Mathf.CeilToInt(amount / dirtPerParticle));
            float perParticleAmount = amount / particleCount;

            for (int i = 0; i < particleCount; i++)
            {
                ParticleBase p = pool.Get();
                p.SetPool(pool);
                p.transform.position = backpack.transform.position;
                p.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", data.Color);
                p.PlayAnimation(backpack.transform.position, robotStoraged.transform, perParticleAmount, onArrive);

                spawned++;
                if (spawned % particlesPerFrame == 0)
                {
                    yield return null;
                }
            }
        }
    }
    public void OnEnterRobot()
    {
        movementParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        groundedParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
    [ContextMenu("PlayerBackPack VFX Debug")]
    public void DebugBackPack()
    {
        foreach (var a in collectedDirtByType)
        {
            Debug.Log(a + "Key: " + "Value -> " + a.Value);
        }
    }

    void OnEnable()
    {
        animationController.enabled = true;
        toolController.enabled = true;
        InputManager.OnJump += Jump;
    }

    void OnDisable()
    {
        InputManager.OnJump -= Jump;
    }

}