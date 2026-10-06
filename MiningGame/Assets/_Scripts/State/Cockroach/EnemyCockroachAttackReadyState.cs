using UnityEngine;

public class EnemyCockroachAttackReadyState : EnemyCockroachState
{
    private float attackWaitState = .6f;
    private float counter;
    public EnemyCockroachAttackReadyState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player) : base(stateMachine, owner, player)
    {
    }
    public override void Enter()
    {
        base.Enter();
        if (!Owner.SpawnedManager.IsPlayerOnDungeon())
        {
            return;
        }
        else
        {
            Owner.AttackReadyStateSet(true);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Owner.AttackReadyStateSet(false);
        counter = 0f;
    }
    public override void Update()
    {
        base.Update();
        counter += Time.deltaTime;
        if (counter >= attackWaitState)
        {
            StateMachine.ChangeState(Owner.AttackState);
            counter = 0;
        }
    }

}
