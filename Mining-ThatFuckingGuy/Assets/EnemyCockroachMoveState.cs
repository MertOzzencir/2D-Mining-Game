using System.Data;
using UnityEngine;

public class EnemyCockroachMoveState : EnemyCockroachState
{
    private float speed;
    private Vector3 currentDestination;

    public EnemyCockroachMoveState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player, float speed) : base(stateMachine, owner, player)
    {
        this.speed = speed;
    }

    public override void Enter()
    {
        base.Enter();
        PickNextDestination(); // hedefi SADECE state'e girerken bir kere seç
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        Vector3 moveDirection = (currentDestination - Owner.transform.position).normalized;
        Owner.transform.position += moveDirection * speed * Time.deltaTime;

        if (Vector3.Distance(Owner.transform.position, Player.CurrentPosition()) < .5f)
            StateMachine.ChangeState(Owner.IdleState);
        if (Vector3.Distance(Owner.transform.position, currentDestination) < 0.1f)
        {
            PickNextDestination();
        }
    }

    private void PickNextDestination()
    {
        Vector3 directionToPlayer = (Player.CurrentPosition() - Owner.transform.position).normalized;
        Vector3 nextPosition = Owner.transform.position + directionToPlayer;
        BlockData block = PlayerController.CurrentDungeon.GetBlockFromWorldPosition(nextPosition, out _);
        currentDestination = block.WorldPosition;
    }
}