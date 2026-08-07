using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public float Speed;
    public float Damage;
    public StateMachine StateMachine { get; set; }
    public PlayerController Player { get; set; }

    public virtual void Awake()
    {
        StateMachine = new StateMachine();
        Player = FindAnyObjectByType<PlayerController>();
        InitilizeStates();
    }
    public void Start()
    {
    }
    public void Update()
    {
        StateMachine.UpdateState();
    }
    public abstract void InitilizeStates();

}
