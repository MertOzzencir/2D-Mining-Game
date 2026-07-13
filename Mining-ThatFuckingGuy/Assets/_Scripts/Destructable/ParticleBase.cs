using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ParticleBase : MonoBehaviour
{
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float sCurveHeight = 1.5f;
    [SerializeField] private float scaleMultiplierMax;
    public void PlayAnimation(Vector3 startPos, Transform endPos, float dirtAmount, Action<float, ParticleBase> currentEvent)
    {
        transform.localScale = UnityEngine.Random.Range(1, scaleMultiplierMax) * Vector3.one;
        transform.eulerAngles = new Vector3(UnityEngine.Random.Range(0, 180), UnityEngine.Random.Range(0, 180), UnityEngine.Random.Range(0, 180));
        StartCoroutine(CollectRoutine(startPos, endPos, dirtAmount, currentEvent));
    }
    private IEnumerator CollectRoutine(Vector3 start, Transform end, float dirtAmount, Action<float, ParticleBase> currentEvent)
    {


        float elapsed = 0f;
        start += Vector3.right;
        int directionMultiplier = transform.position.y - end.transform.position.y > 0 ? -1 : 1;
        while (elapsed < duration)
        {
            Vector3 direction = (end.position - start);
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.right).normalized;

            Vector3 p1 = start + direction * 0.33f + perpendicular * sCurveHeight * directionMultiplier;
            Vector3 p2 = start + direction * 0.66f - perpendicular * sCurveHeight * directionMultiplier;
            elapsed += Time.deltaTime;
            float t = speedCurve.Evaluate(elapsed / duration);
            transform.position = CubicBezier(start, p1, p2, end.position, t);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 1.5f * Time.deltaTime);
            transform.Rotate(elapsed * Time.deltaTime * 15f, t * Time.deltaTime * 15f, 15f * 15f * Time.deltaTime);
            yield return null;
        }

        transform.position = end.position;
        currentEvent?.Invoke(dirtAmount, this);
        gameObject.SetActive(false);
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

