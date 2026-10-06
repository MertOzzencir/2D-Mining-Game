using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryInformationPiece : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dropName;
    [SerializeField] private TextMeshProUGUI dropCount;

    private Image sprite;
    private DropSO currentDropData;
    private int currentAmount;
    void Awake()
    {
        sprite = GetComponent<Image>();
    }

    public void SetDropDataOnUI(DropSO newData, int amount)
    {
        currentDropData = newData;
        currentAmount += amount;
        DrawUI(newData.UI_Name, currentAmount, newData.UI_Image);
    }
    public void DrawUI(string dropNameNow, int currentAmountNow, Sprite spriteNow)
    {
        dropName.text = dropNameNow;
        dropCount.text = currentAmountNow.ToString();
        sprite.sprite = spriteNow;
    }
    public void Reset()
    {
        currentAmount = 0;
        currentDropData = null;
        dropName.text = "";
        dropCount.text = "";
        sprite.sprite = null;
    }
    public bool HasEmpty()
    {
        return currentDropData == null;
    }
    public DropSO GetCurrentDrop()
    {
        return currentDropData;
    }
}
