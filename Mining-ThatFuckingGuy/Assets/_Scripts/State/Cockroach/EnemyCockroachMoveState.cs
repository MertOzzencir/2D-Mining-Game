using UnityEngine;

public class EnemyCockroachMoveState : EnemyCockroachState
{
    private Vector3 currentDestination;
    private float playerFindDistance;
    public EnemyCockroachMoveState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player, float findDistance) : base(stateMachine, owner, player)
    {
        playerFindDistance = findDistance;
    }

    public override void Enter()
    {
        base.Enter();
        Owner.DiveState.SetDynamicState(this);
        FindClosestBlock();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (!Owner.SpawnedManager.IsPlayerOnDungeon())
        {
            StateMachine.ChangeState(Owner.ReturnBaseState);
            return;
        }
        Vector3 moveDirection = (currentDestination - Owner.transform.position).normalized;
        Owner.transform.position += moveDirection * Owner.Speed * Time.deltaTime;
        Quaternion lookrotation = Quaternion.LookRotation(moveDirection, Vector3.right);
        Owner.transform.rotation = Quaternion.Lerp(Owner.transform.rotation, lookrotation, 25f * Time.deltaTime);
        if (Owner.SpawnedManager.GetPlayer() != null)
        {
            if (Vector3.Distance(Owner.transform.position, Owner.SpawnedManager.GetPlayerPosition()) < playerFindDistance)
                StateMachine.ChangeState(Owner.AttackReadyState);
        }
        if (Vector3.Distance(Owner.transform.position, currentDestination) < 0.1f)
        {
            FindClosestBlock();
        }
    }

    private void FindClosestBlock()
    {
        if (!Owner.SpawnedManager.IsPlayerOnDungeon()) return;
        Vector3 directionToPlayer = (Owner.SpawnedManager.GetPlayerPosition() - Owner.transform.position).normalized;
        Vector3 nextPosition = Owner.transform.position + directionToPlayer;
        BlockData block = Owner.SpawnedManager.GetBlockFromWorldPosition(nextPosition, out _);
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