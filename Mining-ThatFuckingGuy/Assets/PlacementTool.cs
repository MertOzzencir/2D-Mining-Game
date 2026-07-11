using UnityEngine;

public class PlacementTool : ToolBase
{
    [SerializeField] private GameObject cornerPrefab;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private PlacementToolBall ballPrefab;
    DungeonManager dungeonManager;
    private int rotationIndex;
    private BlockData currentBlock;
    private Transform pivotTransform;
    private GameObject corner;
    private GameObject indicator;
    public override void Awake()
    {
        base.Awake();
        corner = Instantiate(cornerPrefab);
        indicator = Instantiate(indicatorPrefab);
        dungeonManager = DungeonManager.Instance;
        pivotTransform = corner.transform.GetChild(0).transform;
    }
    public override void UpdateUse()
    {
        base.UpdateUse();
        if (AlternativeState)
        {
            Plane plane = new Plane(Vector3.right, dungeonManager.transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                BlockData checking = dungeonManager.GetBlockFromWorldPosition(hitPoint, out bool isEmpty);
                if (checking != currentBlock)
                {
                    currentBlock = checking;
                    if (isEmpty)
                    {
                        corner.transform.position = currentBlock.WorldPosition;
                        indicator.transform.position = currentBlock.WorldPosition;
                    }
                    bool success = false;
                    int repeat = 0;
                    while (!success)
                    {
                        Debug.Log("While");
                        RotationLogic(out success, rotationIndex);
                        if (!success)
                        {
                            rotationIndex += 1;
                            if (rotationIndex > 3)
                                rotationIndex = 0;
                        }
                        if (repeat > 4) break;
                        repeat++;
                    }
                }
            }
        }
        else
            currentBlock = null;
    }
    public override void UpgradeSelf(UpgradeData upgradeData)
    {

    }
    public override void MainUse(bool state)
    {
        base.MainUse(state);
        Debug.Log("count");
        if (!state) return;
        if (!AlternativeState)
        {

            PlacementToolBall a = Instantiate(ballPrefab, AimPositionTransform.position, Quaternion.identity);
            a.SetDirection(AimPositionTransform.forward);
            return;
        }
        if (currentBlock.CornerIndex[rotationIndex] == -1)
        {
            currentBlock.CornerIndex[rotationIndex] = 1;
            Instantiate(corner, corner.transform.position, corner.transform.rotation);
            bool success = false;
            int repeat = 0;
            while (!success)
            {
                repeat++;
                RotationIncrease(out success);
                if (repeat > 4) break;
            }
        }
    }
    public override void AlternativeUse(bool state)
    {
        base.AlternativeUse(state);
        if (state)
        {
            corner.SetActive(true);
            indicator.SetActive(true);
        }
        else
        {
            indicator.SetActive(false);
            corner.SetActive(false);
        }
    }
    public override void InteractUse()
    {
        base.InteractUse();
        int repeat = 0;
        bool success = false;
        if (AlternativeState)
        {
            if (!MainUseState)
            {
                while (!success)
                {
                    repeat++;
                    RotationIncrease(out success);
                    if (repeat > 4) break;
                }
            }
        }
    }

    private void RotationIncrease(out bool success)
    {
        rotationIndex++;
        if (rotationIndex > 3)
            rotationIndex = 0;
        success = false;
        RotationLogic(out success, rotationIndex);
    }

    private bool RotationLogic(out bool success, int rotationIndex)
    {
        success = false;
        switch (rotationIndex)
        {
            case 0:
                if (dungeonManager.GetEmptyBlockFromWorldPosition(currentBlock, 1, 0) && currentBlock.CornerIndex[rotationIndex] == -1)
                {
                    pivotTransform.transform.localEulerAngles = new Vector3(0, 0, 0);
                    success = true;
                }
                break;
            case 1:
                if (dungeonManager.GetEmptyBlockFromWorldPosition(currentBlock, 0, -1) && currentBlock.CornerIndex[rotationIndex] == -1)
                {
                    pivotTransform.transform.localEulerAngles = new Vector3(90, 0, 0);
                    success = true;
                }
                break;
            case 2:
                if (dungeonManager.GetEmptyBlockFromWorldPosition(currentBlock, -1, 0) && currentBlock.CornerIndex[rotationIndex] == -1)
                {
                    pivotTransform.transform.localEulerAngles = new Vector3(180, 0, 0);
                    success = true;
                }
                break;
            case 3:
                if (dungeonManager.GetEmptyBlockFromWorldPosition(currentBlock, 0, 1) && currentBlock.CornerIndex[rotationIndex] == -1)
                {
                    pivotTransform.transform.localEulerAngles = new Vector3(270, 0, 0);
                    success = true;
                }
                break;
        }

        return success;
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (corner != null && indicator != null)
        {
            corner.SetActive(false);
            indicator.SetActive(false);
        }
    }

}
