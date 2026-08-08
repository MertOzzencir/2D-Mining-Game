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
    public override void Update()
    {
        base.Update();

    }
    private void AttackEnd()
    {
        if (Player == null)
        {
            StateMachine.ChangeState(Owner.ReturnBaseState);
            return;
        }
        Player.GetHealthController().TakeDamage(Owner.Damage);
        StateMachine.ChangeState(Owner.IdleState);
    }

}
