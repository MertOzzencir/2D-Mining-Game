using UnityEngine;

public class VerticalCorrection : MonoBehaviour
{
    [SerializeField] private Vector3 axisCorrection;
    public static PlayerController Player;
    private Vector3 startPosition;
    void Awake()
    {
        if (Player == null)
            Player = FindAnyObjectByType<PlayerController>();

        startPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 camera = Player.GetCamera().transform.position;
        Vector3 target = startPosition + new Vector3(0, camera.y * axisCorrection.y, camera.z * axisCorrection.z);
        transform.position = target;//Vector3.Lerp(transform.position, target,100*Time.deltaTime);
    }
}
