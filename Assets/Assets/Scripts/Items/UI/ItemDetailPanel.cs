using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Default docked item view, living inside ContextWindow: shows a PlacedItem's full details -
// big icon, name, type, stacked quantity (if stackable), and description. Hidden by default;
// shown/populated by InventoryUIController.ShowItemView() on left-click, hidden by CloseItemView().
public class ItemDetailPanel : MonoBehaviour
{
    public Image bigIconImage;
    public AspectRatioFitter bigIconFitter;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI descriptionText;

    // Vertical breathing room kept between this panel and the item it's pointing at.
    private const float TargetGap = 8f;

    private RectTransform rectTransform;
    private RectTransform screenBounds;

    void Awake()
    {
        rectTransform = (RectTransform)transform;
        Canvas canvas = GetComponentInParent<Canvas>(true);
        screenBounds = canvas != null ? (RectTransform)canvas.rootCanvas.transform : null;
    }

    // Repositions this panel directly above targetRect (the InventoryItemView just selected),
    // horizontally centered on it. Assumes this panel's own RectTransform uses a top-left
    // pivot (as set up on the DetailPanel prefab object - see InventoryItemView's
    // UpdateVisualPosition for the equivalent center-pivot assumption elsewhere), so
    // rectTransform.position directly IS the panel's top-left corner in world space.
    // Falls back to docking below the item, and always clamps horizontally, so the panel
    // stays fully on screen even when the item is near a screen edge.
    public void PositionAbove(RectTransform targetRect)
    {
        if (targetRect == null) return;
        if (rectTransform == null) rectTransform = (RectTransform)transform;

        Vector3[] targetCorners = new Vector3[4];
        targetRect.GetWorldCorners(targetCorners);
        Vector3 topCenter = (targetCorners[1] + targetCorners[2]) / 2f;
        Vector3 bottomCenter = (targetCorners[0] + targetCorners[3]) / 2f;

        Vector2 size = rectTransform.rect.size;
        Vector3 scale = rectTransform.lossyScale;
        float width = size.x * scale.x;
        float height = size.y * scale.y;

        float left = topCenter.x - width / 2f;
        float top = topCenter.y + TargetGap + height;

        if (screenBounds != null)
        {
            Vector3[] boundsCorners = new Vector3[4];
            screenBounds.GetWorldCorners(boundsCorners);
            float boundsLeft = boundsCorners[0].x;
            float boundsRight = boundsCorners[2].x;
            float boundsTop = boundsCorners[1].y;
            float boundsBottom = boundsCorners[0].y;

            // Not enough room above the item on screen - dock below it instead, still no
            // closer to the bottom edge than the panel's own height allows.
            if (top > boundsTop) top = Mathf.Max(bottomCenter.y - TargetGap, boundsBottom + height);

            left = Mathf.Clamp(left, boundsLeft, boundsRight - width);
        }

        rectTransform.position = new Vector3(left, top, rectTransform.position.z);
    }

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
