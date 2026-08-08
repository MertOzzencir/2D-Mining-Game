using System;
using System.Collections;
using UnityEngine;

public class CockroachEnemy : Enemy
{
    [SerializeField] private Vector2 attackDurationRandomBoundaries;
    [SerializeField] private Vector2 playerFindDistanceRandomBoundaries;
    [SerializeField] private float xOffSet;
    [SerializeField] private float offSetFromPlayerToSelf;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float scaleAnimationTime;

    public EnemyCockroachIdleState IdleState;
    public EnemyCockroachMoveState MoveState;
    public EnemyCockroachMoveDiveState DiveState;
    public EnemyCockroachAttackState AttackState;
    public EnemyCockroachBackToBaseState ReturnBaseState;
    public EnemyCockroachInactiveState InactiveState;

    public BlockPoint PreviousPoint { get; set; }
    public BlockData CurrentBlock { get; set; }
    public BlockData PreviousBlock { get; set; }
    public CockroachBase BaseOwner { get; set; }
    public float PlayerFindDistance { get; private set; }
    public float AttackDuration { get; private set; }

    // Gizmos için önbelleğe alınan ray bilgisi
    private Vector3 debugRayOrigin;
    private Vector3 debugRayDirection;
    private bool hasDebugRay;

    [ContextMenu("Find Navigation")]
    public override void Awake()
    {
        // Bu ikisi SADECE BURADA, spawn anında bir kere belirleniyor - artık bir daha hiç değişmiyor
        PlayerFindDistance = UnityEngine.Random.Range(playerFindDistanceRandomBoundaries.x, playerFindDistanceRandomBoundaries.y);
        AttackDuration = UnityEngine.Random.Range(attackDurationRandomBoundaries.x, attackDurationRandomBoundaries.y);

        Vector3 nextPosition = transform.position;
        BlockData block = PlayerController.CurrentDungeon.GetBlockFromWorldPosition(nextPosition, out _);
        CurrentBlock = block;
        PreviousBlock = block;
        PreviousPoint = CurrentBlock.FindClosestFrontPoint(transform.position);
        base.Awake(); // InitilizeStates() burada çağrılıyor, PlayerFindDistance'ın YUKARIDA hazır olması şart
    }

    public override void InitilizeStates()
    {
        IdleState = new EnemyCockroachIdleState(StateMachine, this, Player);
        MoveState = new EnemyCockroachMoveState(StateMachine, this, Player, PlayerFindDistance);
        DiveState = new EnemyCockroachMoveDiveState(StateMachine, this, Player);
        AttackState = new EnemyCockroachAttackState(StateMachine, this, Player);
        ReturnBaseState = new EnemyCockroachBackToBaseState(StateMachine, this, Player);
        InactiveState = new EnemyCockroachInactiveState(StateMachine, this, Player);
        StateMachine.Initilize(InactiveState);
        InactiveState.ActiveSelf();
    }

    public void ScaleAnimationStart(Vector3 startScale, Vector3 targetScale)
    {
        StartCoroutine(ScaleAnimation(startScale, targetScale));
    }

    public void AttackAnimationStart(Vector3 startPos, Transform endPos, Action animationEndConditionEvent)
    {
        StartCoroutine(AttackAnimation(startPos, endPos, animationEndConditionEvent));
    }

    private IEnumerator ScaleAnimation(Vector3 startScale, Vector3 targetScale)
    {
        float timer = 0;
        while (timer <= scaleAnimationTime)
        {
            timer += Time.deltaTime;
            float t = timer / scaleAnimationTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        timer = 0;
        float animationScaled = scaleAnimationTime / 2;
        while (timer <= animationScaled)
        {
            timer += Time.deltaTime;
            float t = timer / animationScaled;
            transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            yield return null;
        }
    }

    private IEnumerator AttackAnimation(Vector3 start, Transform end, Action animationEnd)
    {

        Vector3 xOffSetPosition = new Vector3(end.transform.position.x + xOffSet, end.transform.position.y, end.transform.position.z);
        Vector3 newDirection = (end.transform.position - xOffSetPosition).normalized;
        Vector3 jumpPosition = end.position;
        Vector3 endPosition = end.position;
        bool success = false;
        float elapsed = 0f;


        Ray ray = new Ray(xOffSetPosition, newDirection);

        debugRayOrigin = ray.origin;
        debugRayDirection = ray.direction;
        hasDebugRay = true;

        RaycastHit[] hits = Physics.RaycastAll(ray);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        Vector3 directionToEnemy = (transform.position - end.position).normalized;

        foreach (var a in hits)
        {
            if (a.collider.GetComponent<PlayerController>())
            {
                jumpPosition = a.point + directionToEnemy * offSetFromPlayerToSelf;
            }
        }
        Quaternion lookRotation = Quaternion.LookRotation(-directionToEnemy, Vector3.right);

        while (elapsed < AttackDuration)
        {
            if (end == null)
            {
                success = false;
                break;
            }
            if (Vector3.Distance(endPosition, end.position) >= PlayerFindDistance + 0.1f)
            {
                success = false;
                break;
            }
            success = true;
            elapsed += Time.deltaTime;
            float t = elapsed / AttackDuration;
            transform.position = Vector3.Lerp(start, new Vector3(jumpPosition.x + speedCurve.Evaluate(t), jumpPosition.y, jumpPosition.z), t);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, t);
            yield return null;
        }

        animationEnd?.Invoke();

        if (success)
        {
            transform.position = jumpPosition;
        }
    }

    void OnDrawGizmos()
    {
        if (!hasDebugRay) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(debugRayOrigin, 0.1f);
        Gizmos.DrawRay(debugRayOrigin, debugRayDirection * PlayerFindDistance);
    }

    public void OnSpawned(CockroachBase ownerBase)
    {
        BaseOwner = ownerBase;
    }
}