using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private PlayerController controller;
    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }
    void Update()
    {
        Debug.Log(controller.GetAnyDirectionVelocity());
        anim.SetFloat("velocity", controller.GetAnyDirectionVelocity());
    }
}
