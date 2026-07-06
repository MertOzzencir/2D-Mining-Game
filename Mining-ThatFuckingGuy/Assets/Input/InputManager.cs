using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static event Action<bool> OnMouseLeft;
    public static event Action<bool> OnMouseRight;
    public static event Action<int> OnNumbers;
    public static event Action OnJump;
    private InputActionBase baseInput;


    private void Awake()
    {
        baseInput = new InputActionBase();
        baseInput.Enable();
    }

    public Vector2 MovementVectorNormalized()
    {
        Vector2 moveInput = baseInput.Player.Move.ReadValue<Vector2>();
        return moveInput;
    }
    private void MouseLeft(InputAction.CallbackContext context)
    {
        bool isActive = context.phase == InputActionPhase.Performed ? true : false;
        OnMouseLeft?.Invoke(isActive);
    }
    private void SpaceButton(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }
    private void Numbers(InputAction.CallbackContext context)
    {
        int returned = 0;
        switch (context.control.name)
        {
            case "1":
                returned = 1;
                break;
            case "2":
                returned = 2;
                break;
            case "3":
                returned = 3;
                break;
            case "4":
                returned = 4;
                break;
            case "5":
                returned = 5;
                break;
            case "6":
                returned = 6;
                break;
            case "7":
                returned = 7;
                break;
            case "8":
                returned = 8;
                break;
            case "9":
                returned = 9;
                break;

        }
        OnNumbers?.Invoke(returned);
    }
    void OnEnable()
    {
        baseInput.Player.Numbers.performed += Numbers;
        baseInput.Player.MouseLeft.performed += MouseLeft;
        baseInput.Player.MouseLeft.canceled += MouseLeft;
        baseInput.Player.MouseRight.performed += MouseRight;
        baseInput.Player.MouseRight.canceled += MouseRight;
        baseInput.Player.Jump.performed += SpaceButton;
    }

    private void MouseRight(InputAction.CallbackContext context)
    {
        bool isActive = context.phase == InputActionPhase.Performed ? true : false;
        OnMouseRight?.Invoke(isActive);
    }

    void OnDisable()
    {
        baseInput.Player.Numbers.performed -= Numbers;
        baseInput.Player.MouseLeft.performed -= MouseLeft;
        baseInput.Player.MouseLeft.canceled -= MouseLeft;
        baseInput.Player.MouseRight.performed += MouseRight;
        baseInput.Player.MouseRight.canceled += MouseRight;
        baseInput.Player.Jump.performed -= SpaceButton;
    }


}
