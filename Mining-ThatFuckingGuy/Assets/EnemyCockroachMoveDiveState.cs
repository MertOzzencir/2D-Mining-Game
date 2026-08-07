using UnityEngine;

public class EnemyCockroachMoveDiveState : EnemyCockroachState
{
    private Vector3 currentDestination;
    public EnemyCockroachMoveDiveState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player) : base(stateMachine, owner, player)
    {
    }
    public override void Enter()
    {
        base.Enter();
        Dive();
    }
    public override void Exit()
    {
        base.Exit();
        Owner.PreviousBlock = Owner.CurrentBlock;
    }
    public override void Update()
    {
        base.Update();
        Vector3 moveDirection = (currentDestination - Owner.transform.position).normalized;
        Owner.transform.position += moveDirection * Owner.Speed * Time.deltaTime;
        Quaternion lookrotation = Quaternion.LookRotation(moveDirection, Vector3.right);
        Owner.transform.rotation = Quaternion.Lerp(Owner.transform.rotation, lookrotation, 25f * Time.deltaTime);
        if (Vector3.Distance(Owner.transform.position, currentDestination) < 0.1f)
        {
            StateMachine.ChangeState(Owner.MoveState);
        }
    }

    private void Dive()
    {
        Owner.PreviousPoint = Owner.PreviousBlock.GetReverse(Owner.PreviousPoint);
        currentDestination = Owner.PreviousBlock.GetWorldPoint(Owner.PreviousPoint);
        //Owner.ScaleAnimationStart(Owner.transform.localScale, Vector3.zero);
    }
}
