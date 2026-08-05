using UnityEngine;

public class EnemyCockroachState : EnemyStateBase
{
    public CockroachEnemy Owner { get; set; }
    
    public EnemyCockroachState(StateMachine stateMachine, CockroachEnemy owner,PlayerController player) : base(stateMachine,player)
    {
        Owner = owner;
    }

}
