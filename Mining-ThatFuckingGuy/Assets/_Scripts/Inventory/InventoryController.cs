using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryInformationPiece[] uiPieces;

    public void AddDrop(DropSO newDrop, int amount)
    {
        foreach (var piece in uiPieces)
        {
            if (!piece.HasEmpty() && piece.GetCurrentDrop() == newDrop)
            {
                piece.SetDropDataOnUI(newDrop, amount);
                return;
            }
        }

        foreach (var piece in uiPieces)
        {
            if (piece.HasEmpty())
            {
                piece.SetDropDataOnUI(newDrop, amount);
                return;
            }
        }
    }
}