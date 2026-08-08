using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public Vector2 SpeedRandomBoundaries;
    public Vector2 DamageRandomBoundaries;
    public float Speed => Random.Range(SpeedRandomBoundaries.x, SpeedRandomBoundaries.y);
    public float Damage => Random.Range(DamageRandomBoundaries.x,DamageRandomBoundaries.y);
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