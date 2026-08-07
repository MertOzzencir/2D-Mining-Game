using System.Collections;
using UnityEngine;

public class CockroachEnemy : Enemy
{
    [SerializeField] private float scaleAnimationTime;

    public EnemyCockroachIdleState IdleState;
    public EnemyCockroachMoveState MoveState;
    public EnemyCockroachMoveDiveState DiveState;
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
        MoveState = new EnemyCockroachMoveState(StateMachine, this, Player);
        DiveState = new EnemyCockroachMoveDiveState(StateMachine, this, Player);
        StateMachine.Initilize(MoveState);
    }
    public void ScaleAnimationStart(Vector3 startScale, Vector3 targetScale)
    {
        StartCoroutine(ScaleAnimation(startScale, targetScale));
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

}
