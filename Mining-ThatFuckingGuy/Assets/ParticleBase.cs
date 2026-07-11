using System.Collections;
using UnityEngine;

public class ParticleBase : MonoBehaviour
{
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float sCurveHeight = 1.5f;
    public void PlayAnimation(Vector3 startPos, Transform endPos)
    {
        StartCoroutine(CollectRoutine(startPos, endPos));
    }
    private IEnumerator CollectRoutine(Vector3 start, Transform end)
    {


        float elapsed = 0f;
        while (elapsed < duration)
        {
            Vector3 direction = (end.position - start);
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.right).normalized;

            Vector3 p1 = start + direction * 0.33f + perpendicular * sCurveHeight;
            Vector3 p2 = start + direction * 0.66f - perpendicular * sCurveHeight;
            elapsed += Time.deltaTime;
            float t = speedCurve.Evaluate(elapsed / duration);
            transform.position = CubicBezier(start, p1, p2, end.position, t);
            yield return null;
        }

        transform.position = end.position;
        Destroy(gameObject);
    }

    private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return (u * u * u) * p0
             + 3f * (u * u) * t * p1
             + 3f * u * (t * t) * p2
             + (t * t * t) * p3;
    }
}

