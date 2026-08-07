using UnityEngine;

public class EnemyCockroachMoveState : EnemyCockroachState
{
    private Vector3 currentDestination;
    public EnemyCockroachMoveState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player) : base(stateMachine, owner, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        FindClosestBlock();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        Vector3 moveDirection = (currentDestination - Owner.transform.position).normalized;
        Owner.transform.position += moveDirection * Owner.Speed * Time.deltaTime;
        Quaternion lookrotation = Quaternion.LookRotation(moveDirection, Vector3.right);
        Owner.transform.rotation = Quaternion.Lerp(Owner.transform.rotation, lookrotation, 25f * Time.deltaTime);
        if (Vector3.Distance(Owner.transform.position, Player.CurrentPosition()) < .5f)
            StateMachine.ChangeState(Owner.IdleState);
        else if (Vector3.Distance(Owner.transform.position, currentDestination) < 0.1f)
        {
            FindClosestBlock();
        }
    }

    private void FindClosestBlock()
    {
        Vector3 directionToPlayer = (Player.CurrentPosition() - Owner.transform.position).normalized;
        Vector3 nextPosition = Owner.transform.position + directionToPlayer;
        BlockData block = PlayerController.CurrentDungeon.GetBlockFromWorldPosition(nextPosition, out _);
        Owner.CurrentBlock = block;
        if (block.IsEmpty)
        {
            if (Owner.CurrentBlock.IsFront(Owner.PreviousPoint))
            {
                StateMachine.ChangeState(Owner.DiveState);
                return;
            }
            else
            {
                Owner.PreviousPoint = Owner.CurrentBlock.FindClosestBackPoint(Owner.transform.position);
            }
            currentDestination = Owner.CurrentBlock.GetWorldPoint(Owner.PreviousPoint);
            Owner.PreviousBlock = Owner.CurrentBlock;
            return;
        }
        if (Owner.CurrentBlock.IsBack(Owner.PreviousPoint))
        {
            StateMachine.ChangeState(Owner.DiveState);
            return;
        }

        Owner.PreviousPoint = Owner.CurrentBlock.FindClosestFrontPoint(Owner.transform.position);
        currentDestination = Owner.CurrentBlock.GetWorldPoint(Owner.PreviousPoint);
        //Owner.ScaleAnimationStart(Owner.transform.localScale, Vector3.zero);
        Owner.PreviousBlock = Owner.CurrentBlock;
    }

}