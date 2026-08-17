using UnityEngine;

public class EnemyCockroachAttackState : EnemyCockroachState
{
    bool checkThreshhold;
    Vector3 lastPlayerPosition;
    Vector3 jumpedDirection;
    public EnemyCockroachAttackState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player) : base(stateMachine, owner, player)
    {
    }

    public override void Enter()
    {
        base.Enter();
        checkThreshhold = false;
        if (Player != null)
            Owner.AttackAnimationStart(Owner.transform.position, Player.transform, AttackEnd);
    }
    public override void Exit()
    {
        base.Exit();

    }
    public override void Update()
    {
        base.Update();
        if (Owner.Player == null)
        {
            StateMachine.ChangeState(Owner.IdleState);
            return;
        }
        if (checkThreshhold)
        {
            Owner.transform.position = jumpedDirection + Player.transform.position;
            if (Vector3.Distance(Owner.transform.position, lastPlayerPosition) > Owner.AttackAnimationCancelThreshold * 2)
            {
                StateMachine.ChangeState(Owner.IdleState);
            }
        }
    }
    private void AttackEnd(bool s, Vector3 attackPosition, Vector3 jumpDirection)
    {
        if (!Owner.SpawnedManager.IsPlayerOnDungeon())
        {
            StateMachine.ChangeState(Owner.ReturnBaseState);
            return;
        }
        Owner.AttackReadyStateSet(false);
        if (s)
        {
            Player.GetHealthController().TakeDamage(Owner.Damage);
            checkThreshhold = true;
            lastPlayerPosition = attackPosition;
            jumpedDirection = jumpDirection;
        }
        else
        {
            StateMachine.ChangeState(Owner.IdleState);
        }

    }

}
