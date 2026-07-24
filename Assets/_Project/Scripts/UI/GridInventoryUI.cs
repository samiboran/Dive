using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Tek bir grid envanter paneli (oyuncu çantası VEYA stash).
/// Item ikonlarını GridWidth/Height kadar hücre kaplayacak şekilde çizer,
/// drag & drop ile başka panele veya panel içine taşıma sağlar.
///
/// Kurulum: Canvas altında bir Panel'e konur. cellSize + grid ölçüleri
/// kaynak sistemden (InventorySystem / StashUIAdapter) alınır.
/// </summary>
public class GridInventoryUI : MonoBehaviour
{
    [Header("Grid Visual")]
    [SerializeField] private float cellSize = 64f;
    [SerializeField] private Sprite cellBackground;
    [SerializeField] private Color cellColor = new Color(1f, 1f, 1f, 0.1f);

    private RectTransform rectTransform;
    private RectTransform itemLayer;   // ikonlar buraya konur (hücrelerin üstü)
    private int gridWidth;
    private int gridHeight;

    private readonly List<DraggableItemIcon> icons = new List<DraggableItemIcon>();

    public RectTransform ItemLayer => itemLayer;
    public float CellSize => cellSize;

    /// <summary>Panel'i grid ölçüleriyle kur.</summary>
    public void Setup(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;

        rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width * cellSize, height * cellSize);

        // Hücre arka planları
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = new GameObject($"Cell_{x}_{y}", typeof(Image));
                cell.transform.SetParent(transform, false);
                var img = cell.GetComponent<Image>();
                img.sprite = cellBackground;
                img.color = cellColor;
                var rt = cell.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1); // sol-üst
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
                rt.sizeDelta = new Vector2(cellSize - 2, cellSize - 2);
            }
        }

        // İkon katmanı
        var layer = new GameObject("ItemLayer", typeof(RectTransform));
        layer.transform.SetParent(transform, false);
        itemLayer = layer.GetComponent<RectTransform>();
        itemLayer.anchorMin = Vector2.zero;
        itemLayer.anchorMax = Vector2.one;
        itemLayer.offsetMin = itemLayer.offsetMax = Vector2.zero;
    }

    /// <summary>Eski ikonları sil, verilen slot listesinden yeniden çiz.</summary>
    public void RefreshIcons(IReadOnlyList<SlotViewData> slotViews, System.Action<SlotViewData, Vector2, GridInventoryUI> onDrop)
    {
        foreach (var icon in icons)
            if (icon != null) Destroy(icon.gameObject);
        icons.Clear();

        foreach (var view in slotViews)
        {
            var iconObj = new GameObject($"Icon_{view.Item.ItemName}", typeof(Image), typeof(DraggableItemIcon));
            iconObj.transform.SetParent(itemLayer, false);

            var img = iconObj.GetComponent<Image>();
            img.sprite = view.Item.Icon;
            img.raycastTarget = true;

            var rt = iconObj.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(view.GridX * cellSize, -view.GridY * cellSize);
            rt.sizeDelta = new Vector2(view.Item.GridWidth * cellSize - 4, view.Item.GridHeight * cellSize - 4);

            var drag = iconObj.GetComponent<DraggableItemIcon>();
            drag.Init(view, this, onDrop);

            icons.Add(drag);
        }
    }

    /// <summary>Ekran noktasını grid hücresine çevirir (dışarıdaysa false).</summary>
    public bool ScreenToCell(Vector2 screenPos, Camera cam, out int cellX, out int cellY)
    {
        cellX = -1;
        cellY = -1;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, cam, out Vector2 local))
            return false;

        // pivot sol-üst kabulü: local.x → sağ, local.y → yukarı
        int x = Mathf.FloorToInt((local.x + gridWidth * cellSize * rectTransform.pivot.x) / cellSize);
        int y = Mathf.FloorToInt((gridHeight * cellSize * (1 - rectTransform.pivot.y) - local.y) / cellSize);

        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return false;

        cellX = x;
        cellY = y;
        return true;
    }
}

/// <summary>UI'ın ihtiyaç duyduğu düz slot görünümü (sistemden bağımsız).</summary>
public struct SlotViewData
{
    public SO_ItemData Item;
    public int Count;
    public int GridX;
    public int GridY;
    public int SourceIndex; // kaynak sistemdeki slot indeksi
}
