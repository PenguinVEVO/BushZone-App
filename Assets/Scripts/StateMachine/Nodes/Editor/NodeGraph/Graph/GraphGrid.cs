using UnityEngine;
using UnityEngine.UIElements;

public class GraphGrid : VisualElement
{
    private Color gridColor = new Color( 0.3f, 0.3f, 0.3f, 0.4f );
    private Color thickLineColor = new Color( 0.3f, 0.3f, 0.3f, 0.8f );
    private float smallGridSpacing = 10f;
    private float largeGridSpacing = 50f;

    private float zoom = 1f;
    private Vector2 pan = Vector2.zero;

    public GraphGrid()
    {
        generateVisualContent += OnGenerateVisualContent;
        RegisterCallback<GeometryChangedEvent>( OnGeometryChanged );
    }

    private void OnGeometryChanged( GeometryChangedEvent evt )
    {
        MarkDirtyRepaint();
    }

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        var rect = contentRect;
        if (rect.width <= 0 || rect.height <= 0)
            return;

        var painter = mgc.painter2D;

        float spacingSmall = smallGridSpacing * zoom;
        float spacingLarge = largeGridSpacing * zoom;

        Vector2 offset = pan / zoom;
        float startXSmall = rect.xMin - offset.x % spacingSmall;
        float startYSmall = rect.yMin - offset.y % spacingSmall;

        painter.lineWidth = 1f;
        painter.strokeColor = gridColor;

        for (float x = startXSmall; x <= rect.xMax; x += spacingSmall)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, rect.yMin));
            painter.LineTo(new Vector2(x, rect.yMax));
            painter.Stroke();
        }

        for (float y = startYSmall; y <= rect.yMax; y += spacingSmall)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, y));
            painter.LineTo(new Vector2(rect.xMax, y));
            painter.Stroke();
        }

        float startXLarge = rect.xMin - offset.x % spacingLarge;
        float startYLarge = rect.yMin - offset.y % spacingLarge;

        painter.lineWidth = 2f;
        painter.strokeColor = thickLineColor;

        for (float x = startXLarge; x <= rect.xMax; x += spacingLarge)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(x, rect.yMin));
            painter.LineTo(new Vector2(x, rect.yMax));
            painter.Stroke();
        }

        for (float y = startYLarge; y <= rect.yMax; y += spacingLarge)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, y));
            painter.LineTo(new Vector2(rect.xMax, y));
            painter.Stroke();
        }
    }

    public void SetZoomState(float zoomLevel, Vector2 panOffset)
    {
        zoom = Mathf.Max(0.01f, zoomLevel);
        pan = panOffset;
        MarkDirtyRepaint();
    }

    public void SetGridSpacing(float smallSpacing, float largeSpacing)
    {
        smallGridSpacing = Mathf.Max(1f, smallSpacing);
        largeGridSpacing = Mathf.Max(smallGridSpacing, largeSpacing);
        MarkDirtyRepaint();
    }

    public void SetGridColors(Color smallLineColor, Color thickLineColor)
    {
        this.gridColor = smallLineColor;
        this.thickLineColor = thickLineColor;
        MarkDirtyRepaint();
    }

    public void SetGridVisibility(bool visible)
    {
        this.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
