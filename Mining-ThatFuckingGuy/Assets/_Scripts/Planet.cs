using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] private float rotateAmountPerMin;

    private float turnAmount;
    private float multiply;
    void Awake()
    {
        multiply = rotateAmountPerMin / 60f;
    }
    void Update()
    {
        turnAmount = Time.deltaTime * multiply;
        transform.Rotate(0, turnAmount, 0);
    }
}
