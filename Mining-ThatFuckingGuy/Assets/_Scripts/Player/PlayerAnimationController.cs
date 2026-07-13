using System.Collections;
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
        anim.SetFloat("velocity", controller.GetAnyDirectionVelocity());
    }
    public void BeforeDisable()
    {
        StartCoroutine(Disable());
    }
    private void OnEnable()
    {

        anim.enabled = true;
    }

    IEnumerator Disable()
    {
        anim.Play("Armature|Idle");
        yield return new WaitForEndOfFrame();
        anim.enabled = false;
        this.enabled = false;
    }
}
