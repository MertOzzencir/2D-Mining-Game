using UnityEngine;

public abstract class EnemyStateBase : StateBase
{
    public PlayerController Player{get;set;}
    public EnemyStateBase(StateMachine stateMachine,PlayerController player) : base(stateMachine)
    {
        Player = player;
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
    }


}
