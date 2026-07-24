using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Sürüklenebilir item ikonu. Drag sırasında Canvas köküne taşınır (her
/// panelin üstünde görünür), bırakınca hedef GridInventoryUI'a haber verir.
/// </summary>
public class DraggableItemIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private SlotViewData viewData;
    private GridInventoryUI sourcePanel;
    private System.Action<SlotViewData, Vector2, GridInventoryUI> onDrop;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Canvas rootCanvas;

    public void Init(SlotViewData data, GridInventoryUI panel,
        System.Action<SlotViewData, Vector2, GridInventoryUI> dropCallback)
    {
        viewData = data;
        sourcePanel = panel;
        onDrop = dropCallback;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        bool handled = false;
        if (eventData.pointerEnter != null)
        {
            var targetPanel = eventData.pointerEnter.GetComponentInParent<GridInventoryUI>();
            if (targetPanel != null)
            {
                onDrop?.Invoke(viewData, eventData.position, targetPanel);
                handled = true;
            }
        }

        if (!handled)
            transform.SetParent(originalParent, true);
    }
}
