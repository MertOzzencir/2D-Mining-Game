
public class EnemyCockroachInactiveState : EnemyCockroachState
{
    public EnemyCockroachInactiveState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player) : base(stateMachine, owner, player)
    {
    }
    public override void Enter()
    {
        base.Enter();
        Owner.gameObject.SetActive(false);
    }
    public override void Exit()
    {
        base.Exit();
        Owner.gameObject.SetActive(true);
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
