
public class EnemyCockroachInactiveState : EnemyCockroachState
{
    public EnemyCockroachInactiveState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player) : base(stateMachine, owner, player)
    {
    }
    public override void Update()
    {
        return;
    }
    public void ActiveSelf()
    {
        StateMachine.ChangeState(Owner.IdleState);
        Owner.IsReturningToBase = false;
    }
}
