using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Fixed side panel showing the currently-selected PlacedItem's full details:
// big icon, name, type, stacked quantity (if stackable), and description.
// Driven by InventoryUIController.SelectItem().
public class ItemDetailPanel : MonoBehaviour
{
    public Image bigIconImage;
    public AspectRatioFitter bigIconFitter;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI descriptionText;

    public void ShowItem(PlacedItem item)
    {
        ItemData data = item.itemData;

        bigIconImage.sprite = data.itemIcon;
        bigIconImage.enabled = data.itemIcon != null;

        if (bigIconFitter != null && data.itemIcon != null)
        {
            Rect rect = data.itemIcon.rect;
            bigIconFitter.aspectRatio = rect.width / rect.height;
        }

        nameText.text = data.itemName;
        typeText.text = data.itemType.ToString();
        quantityText.text = data.isStackable ? $"Quantity: {item.quantity}" : "";
        descriptionText.text = data.itemDescription;
    }

    public void Clear()
    {
        bigIconImage.sprite = null;
        bigIconImage.enabled = false;

        nameText.text = "";
        typeText.text = "";
        quantityText.text = "";
        descriptionText.text = "Select an item to view details.";
    }
}
