

public abstract class StateBase
{
    public StateMachine StateMachine;
    public StateBase(StateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();

}


