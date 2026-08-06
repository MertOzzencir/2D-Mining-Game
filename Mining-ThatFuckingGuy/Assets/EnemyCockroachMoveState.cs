using UnityEngine;

public class EnemyCockroachMoveState : EnemyCockroachState
{
    private float speed;
    private BlockData currentBlock;
    private BlockPoint previousChosenPoint;
    private Vector3 currentDestination;
    private bool preferFront = true; // hangi taraftan seçim yapılacağını takip eden bayrak
    private int randomTurnChance;
    private int randomTurnChanceCounter;
    public EnemyCockroachMoveState(StateMachine stateMachine, CockroachEnemy owner, PlayerController player, float speed) : base(stateMachine, owner, player)
    {
        this.speed = speed;
    }

    public override void Enter()
    {
        base.Enter();
        FindClosestBlock();
        CalculateRandomTurnChance();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        Vector3 moveDirection = (currentDestination - Owner.transform.position).normalized;
        Owner.transform.position += moveDirection * speed * Time.deltaTime;
        Quaternion lookrotation = Quaternion.LookRotation(moveDirection, Vector3.right);
        Owner.transform.rotation = Quaternion.Lerp(Owner.transform.rotation, lookrotation, 25f * Time.deltaTime);
        if (Vector3.Distance(Owner.transform.position, Player.CurrentPosition()) < .5f)
            StateMachine.ChangeState(Owner.IdleState);
        else if (Vector3.Distance(Owner.transform.position, currentDestination) < 0.1f)
        {
            FindClosestBlock();
        }
    }

    private void FindClosestBlock()
    {

        if (randomTurnChanceCounter > randomTurnChance)
        {
            randomTurnChanceCounter = 0;
            CalculateRandomTurnChance();
            preferFront = !preferFront;
            currentDestination = currentBlock.GetWorldPoint(currentBlock.GetReverse(previousChosenPoint));
            return;
        }

        Vector3 directionToPlayer = (Player.CurrentPosition() - Owner.transform.position).normalized;
        Vector3 nextPosition = Owner.transform.position + directionToPlayer;
        BlockData block = PlayerController.CurrentDungeon.GetBlockFromWorldPosition(nextPosition, out _);
        currentBlock = block;

        previousChosenPoint = preferFront
            ? FindClosestFrontPointToBlock(currentBlock)
            : FindClosestBackPointToBlock(currentBlock);

        currentDestination = currentBlock.GetWorldPoint(previousChosenPoint);
        randomTurnChanceCounter++;
    }


    private void CalculateRandomTurnChance()
    {
        randomTurnChance = Random.Range(3, 5);
    }
    private BlockPoint FindClosestFrontPointToBlock(BlockData currentBlock)
    {
        float z = currentBlock.WorldPosition.z - Owner.transform.position.z;
        float y = currentBlock.WorldPosition.y - Owner.transform.position.y;

        const float centerThreshold = 0.5f;
        bool zCenter = Mathf.Abs(z) < centerThreshold;
        bool yCenter = Mathf.Abs(y) < centerThreshold;

        if (zCenter && yCenter)
            return y >= 0 ? BlockPoint.FrontTop : BlockPoint.FrontBottom;

        if (zCenter)
            return y > 0 ? BlockPoint.FrontTop : BlockPoint.FrontBottom;

        if (yCenter)
            return z > 0 ? BlockPoint.FrontRight : BlockPoint.FrontLeft;

        if (z < 0 && y < 0) return BlockPoint.FrontBottomLeft;
        if (z < 0 && y > 0) return BlockPoint.FrontTopLeft;
        if (z > 0 && y < 0) return BlockPoint.FrontBottomRight;
        return BlockPoint.FrontTopRight;
    }

    private BlockPoint FindClosestBackPointToBlock(BlockData currentBlock)
    {
        float z = currentBlock.WorldPosition.z - Owner.transform.position.z;
        float y = currentBlock.WorldPosition.y - Owner.transform.position.y;

        const float centerThreshold = 0.5f;
        bool zCenter = Mathf.Abs(z) < centerThreshold;
        bool yCenter = Mathf.Abs(y) < centerThreshold;

        if (zCenter && yCenter)
            return y >= 0 ? BlockPoint.BackTop : BlockPoint.BackBottom;

        if (zCenter)
            return y > 0 ? BlockPoint.BackTop : BlockPoint.BackBottom;

        if (yCenter)
            return z > 0 ? BlockPoint.BackRight : BlockPoint.BackLeft;

        if (z < 0 && y < 0) return BlockPoint.BackBottomLeft;
        if (z < 0 && y > 0) return BlockPoint.BackTopLeft;
        if (z > 0 && y < 0) return BlockPoint.BackBottomRight;
        return BlockPoint.BackTopRight;
    }
}