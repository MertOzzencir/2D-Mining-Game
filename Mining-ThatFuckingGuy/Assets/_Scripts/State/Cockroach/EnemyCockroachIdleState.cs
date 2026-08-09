using UnityEngine;
using UnityEngine.XR;

public class EnemyCockroachIdleState : EnemyCockroachState
{
    public EnemyCockroachIdleState(StateMachine stateMachine, CockroachEnemy enemy, PlayerController player) : base(stateMachine, enemy, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // StateMachine.ChangeState(Owner.MoveState);

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
        if (Vector3.Distance(Owner.transform.position, Owner.SpawnedManager.GetPlayerPosition()) > 2f)
        {
            StateMachine.ChangeState(Owner.MoveState);
        }
    }
}
