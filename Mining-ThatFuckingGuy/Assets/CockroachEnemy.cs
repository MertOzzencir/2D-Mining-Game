using System.Collections;
using UnityEngine;

public class CockroachEnemy : Enemy
{
    public EnemyCockroachIdleState IdleState;
    public EnemyCockroachMoveState MoveState;

    [ContextMenu("Find Navigation")]
    public override void InitilizeStates()
    {
        IdleState = new EnemyCockroachIdleState(StateMachine, this, Player);
        MoveState = new EnemyCockroachMoveState(StateMachine, this, Player, BlockToBlockTimeSpeed);
        StateMachine.Initilize(MoveState);
    }

}
