using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] private LineRenderer beam;
    [SerializeField] private float visualOffSet;
    private float range;
    float toolXDiff;

    void Awake()
    {
        toolXDiff = transform.localPosition.z - Vector3.zero.z;
    }
    void Update()
    {
        beam.SetPosition(0, transform.position);
        beam.SetPosition(1, transform.position + transform.forward * (range - toolXDiff - visualOffSet));
    }

    public void UpdateRange(float range)
    {
        this.range = range;
    }
}
