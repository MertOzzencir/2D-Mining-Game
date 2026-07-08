using System.Collections;
using UnityEngine;

public class DropBase : MonoBehaviour
{
    [SerializeField] private float slerpTiming;

    [SerializeField] private DropSO data;
    public bool IsCollected { get; private set; }
    private Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Collect(Transform storagedPlaced, float animationDuration)
    {
        if (!IsCollected)
        {
            rb.isKinematic = true;
            StartCoroutine(CollectAnimation(storagedPlaced, transform.position, animationDuration));
            IsCollected = true;
        }
    }

    private IEnumerator CollectAnimation(Transform target, Vector3 startPosition, float animationDuration)
    {

        float duration = animationDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 center = Vector3.Lerp(startPosition, target.position, 0.5f) - Vector3.up * 0.3f; // biraz aşağıda bir merkez
            Vector3 startRelative = startPosition - center;
            Vector3 endRelative = target.position - center;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 arced = Vector3.Slerp(startRelative, endRelative, t);
            transform.position = center + arced;

            yield return null;
        }

        transform.parent = target;
        gameObject.SetActive(false);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }
}
