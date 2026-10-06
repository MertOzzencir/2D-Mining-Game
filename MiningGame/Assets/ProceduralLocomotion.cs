using System;
using System.Collections;
using UnityEngine;

public class ProceduralLocomotion : MonoBehaviour
{
    [SerializeField] private LegData[] legs;
    [SerializeField] private int groupCount = 2;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 offSetAmount;

    private int activeGroup = 0;
    void Awake()
    {
        foreach (var a in legs)
        {
            a.TargetDesireLocalPosition = a.TargetDesire.localPosition;
        }
    }

    private void Update()
    {
        HandleTargetDesirePosition();
        TryStepActiveGroup();
    }

    private void HandleTargetDesirePosition()
    {
        if (Input.GetKey(KeyCode.W))
        {
            foreach (var a in legs)
            {
                if (a.PositionCondition == PositionCondition.Front)
                {
                    a.TargetDesire.localPosition = a.TargetDesireLocalPosition - offSetAmount;
                }
                else
                {
                    a.TargetDesire.localPosition = a.TargetDesireLocalPosition;
                }
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            foreach (var a in legs)
            {
                if (a.PositionCondition == PositionCondition.Back)
                {
                    a.TargetDesire.localPosition = a.TargetDesireLocalPosition + offSetAmount;
                }
                else
                {
                    a.TargetDesire.localPosition = a.TargetDesireLocalPosition + offSetAmount/2;
                }
            }
        }
    }

    private void TryStepActiveGroup()
    {
        bool anyLegWantsStep = false;
        bool anyLegStillStepping = false;

        for (int i = 0; i < legs.Length; i++)
        {
            LegData leg = legs[i];
            if (leg.Group != activeGroup) continue;

            if (leg.IsStepping)
            {
                anyLegStillStepping = true;
                continue;
            }

            float dist = Vector3.Distance(leg.Target.transform.position, leg.TargetDesire.position);
            if (dist > leg.DesireThreshold)
            {
                anyLegWantsStep = true;
                StartCoroutine(StepLeg(leg));
            }
        }

        // Bu gruptaki her bacak sakinse (adım atmıyor, atmak da istemiyor), sıradaki gruba geç
        if (!anyLegWantsStep && !anyLegStillStepping)
        {
            activeGroup = (activeGroup + 1) % groupCount;
        }
    }

    private IEnumerator StepLeg(LegData leg)
    {
        Ray ray = new Ray(leg.TipLeg.transform.position, -leg.TipLeg.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, leg.RayDistanceToFloor))
        {
            yield break; // önünde zemin yok, adım atma
        }

        leg.IsStepping = true;

        Vector3 startPos = leg.Target.transform.position;
        Vector3 endPos = leg.TargetDesire.position;
        Vector3 arcControl = Vector3.Lerp(startPos, endPos, 0.5f) + leg.TipLeg.transform.forward * leg.StepHeight;

        float elapsed = 0f;
        while (elapsed < leg.StepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / leg.StepDuration);

            Vector3 a = Vector3.Lerp(startPos, arcControl, t);
            Vector3 b = Vector3.Lerp(arcControl, endPos, t);
            leg.Target.transform.position = Vector3.Lerp(a, b, t);

            yield return null;
        }

        leg.Target.transform.position = endPos;
        leg.IsStepping = false;
    }
    void OnDrawGizmos()
    {
        foreach (var a in legs)
        {
            Gizmos.DrawCube(a.TargetDesire.position, Vector3.one / 2f);
        }
    }
}

[Serializable]
public class LegData
{
    public PositionCondition PositionCondition;
    public GameObject TipLeg;
    public GameObject Target;
    public Transform TargetDesire;
    public Vector3 TargetDesireLocalPosition;
    public float DesireThreshold;
    public float RayDistanceToFloor;
    public float StepDuration = 0.15f;
    public float StepHeight = 0.3f; // YENİ - adım sırasında ayağın ne kadar yükseleceği
    public int Group; // YENİ - Inspector'dan elle ata: hangi bacaklar birlikte hareket edecek

    [NonSerialized] public bool IsStepping; // runtime state, kaydedilmez
}

[Serializable]
public enum PositionCondition
{
    Front,
    Back
}