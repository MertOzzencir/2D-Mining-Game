using System;
using System.Collections;
using UnityEngine;

public class CockroachEnemy : Enemy
{
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float scaleAnimationTime;
    [SerializeField] private float attackDuration = 0.6f;
    [SerializeField] private float playerFindDistance;
    public EnemyCockroachIdleState IdleState;
    public EnemyCockroachMoveState MoveState;
    public EnemyCockroachMoveDiveState DiveState;
    public EnemyCockroachAttackState AttackState;
    public BlockPoint PreviousPoint { get; set; }
    public BlockData CurrentBlock { get; set; }
    public BlockData PreviousBlock { get; set; }
    [ContextMenu("Find Navigation")]
    public override void Awake()
    {
        Vector3 nextPosition = transform.position;
        BlockData block = PlayerController.CurrentDungeon.GetBlockFromWorldPosition(nextPosition, out _);
        CurrentBlock = block;
        PreviousBlock = block;
        PreviousPoint = CurrentBlock.FindClosestFrontPoint(transform.position);
        base.Awake();
    }
    public override void InitilizeStates()
    {
        IdleState = new EnemyCockroachIdleState(StateMachine, this, Player);
        MoveState = new EnemyCockroachMoveState(StateMachine, this, Player, playerFindDistance);
        DiveState = new EnemyCockroachMoveDiveState(StateMachine, this, Player);
        AttackState = new EnemyCockroachAttackState(StateMachine, this, Player);
        StateMachine.Initilize(MoveState);
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
        bool success = true;
        float elapsed = 0f;
        start += Vector3.right;
        int directionMultiplier = transform.position.y - end.transform.position.y > 0 ? -1 : 1;
        Vector3 endPosition = end.position;
        Vector3 direction = (end.position - transform.position).normalized;
        while (elapsed < attackDuration)
        {
            if (Vector3.Distance(endPosition, end.position) >= playerFindDistance + 0.1f)
            {
                success = false;
                break;
            }

            Quaternion lookRotation = Quaternion.LookRotation(direction, end.forward);
            elapsed += Time.deltaTime;
            float t = elapsed / attackDuration;
            transform.position = Vector3.Lerp(start, new Vector3(end.position.x + speedCurve.Evaluate(t), end.position.y, end.position.z), t);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, t);
            yield return null;
        }
        animationEnd?.Invoke();

        if (success)
        {
            transform.position = end.position;
        }
    }
}
