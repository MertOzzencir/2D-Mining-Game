using UnityEngine;

public class StateMachine
{

    StateBase currentBase;
    public void Initilize(StateBase startState)
    {
        Debug.Log(currentBase);

        currentBase = startState;
        currentBase.Enter();
    }
    public void ChangeState(StateBase next)
    {
        currentBase.Exit();
        currentBase = next;
        currentBase.Enter();
        Debug.Log(currentBase);

    }
    public void UpdateState()
    {
        currentBase.Update();
    }

}
