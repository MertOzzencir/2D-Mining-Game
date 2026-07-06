using UnityEngine;

public class DropBase : MonoBehaviour
{
    [SerializeField] private DropSO data;
    public void Collect(Transform storagedPlaced)
    {
        gameObject.SetActive(false);
        transform.parent = storagedPlaced;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }
}
