using UnityEngine;

public class EnemyCockroachAttackState : EnemyCockroachState
{
    public EnemyCockroachAttackState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player) : base(stateMachine, owner, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Owner.AttackAnimationStart(Owner.transform.position, Player.GetHead(), AttackEnd);
    }
    public override void Exit()
    {
        base.Exit();
    }
    private void AttackEnd()
    {
        if (!Owner.SpawnedManager.IsPlayerOnDungeon())
        {
            StateMachine.ChangeState(Owner.ReturnBaseState);
            return;
        }
        Owner.AttackReadyStateSet(false);
        Player.GetHealthController().TakeDamage(Owner.Damage);
        StateMachine.ChangeState(Owner.IdleState);
    }

}
