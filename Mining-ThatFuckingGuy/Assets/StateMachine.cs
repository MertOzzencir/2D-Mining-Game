using UnityEngine;

public class StateMachine 
{

    StateBase currentBase;
    public void Initilize(StateBase startState)
    {
        currentBase = startState;
        currentBase.Enter();
    }
    public void ChangeState(StateBase next)
    {
        currentBase.Exit();
        currentBase = next;
        currentBase.Enter();
    }
    public void UpdateState()
    {
        //Debug.Log(currentBase);
        currentBase.Update();
    }

}
