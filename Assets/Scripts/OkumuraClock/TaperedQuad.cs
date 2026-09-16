using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Okumura Clock/Tapered Quad")]
[RequireComponent(typeof(CanvasRenderer))]
public class TaperedQuad : MaskableGraphic
{
    [Tooltip("Width of the hand at the base, in the canvas' reference pixels.")]
    [Min(0f)]
    [SerializeField]
    private float _widthAtBase = 21f;

    [Tooltip("Width of the hand at the tip, in the canvas' reference pixels.")]
    [Min(0f)]
    [SerializeField]
    private float _widthAtTip = 12f;

    [Tooltip("Thickness of the border drawn outside the shape, in the canvas' reference pixels.")]
    [Min(0f)]
    [SerializeField]
    private float _borderWidth = 4f;

    [Tooltip("Color of the border. Its alpha is multiplied by the graphic's alpha.")]
    [SerializeField]
    private Color _borderColor = Color.black;

    public float WidthAtBase
    {
        get => _widthAtBase;
        set
        {
            float clamped = Mathf.Max(0f, value);

            if (_widthAtBase == clamped)
                return;

            _widthAtBase = clamped;
            SetVerticesDirty();
        }
    }

    public float WidthAtTip
    {
        get => _widthAtTip;
        set
        {
            float clamped = Mathf.Max(0f, value);

            if (_widthAtTip == clamped)
                return;

            _widthAtTip = clamped;
            SetVerticesDirty();
        }
    }

    public float BorderWidth
    {
        get => _borderWidth;
        set
        {
            float clamped = Mathf.Max(0f, value);

            if (_borderWidth == clamped)
                return;

            _borderWidth = clamped;
            SetVerticesDirty();
        }
    }

    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor.Equals(value))
                return;

            _borderColor = value;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();

        if (rect.height <= 0f)
            return;

        float centerX = rect.center.x;
        float halfBase = _widthAtBase * 0.5f;
        float halfTip = _widthAtTip * 0.5f;

        if (_borderWidth > 0f)
        {
            var (borderHalfBase, borderHalfTip) = OffsetHalfWidths(halfBase, halfTip, rect.height, _borderWidth);

            Color borderColor = _borderColor;
            borderColor.a *= color.a;

            AddQuad(vh, centerX, borderHalfBase, borderHalfTip, rect.yMin - _borderWidth, rect.yMax + _borderWidth, borderColor);
        }

        AddQuad(vh, centerX, halfBase, halfTip, rect.yMin, rect.yMax, color);
    }

    private static (float, float) OffsetHalfWidths(float halfBase, float halfTip, float height, float offset)
    {
        if (height <= 0f)
            return (halfBase + offset, halfTip + offset);

        float delta = halfTip - halfBase;
        float slant = Mathf.Sqrt(height * height + delta * delta);

        return (halfBase + offset * (slant - delta) / height, halfTip + offset * (slant + delta) / height);
    }

    private static void AddQuad(VertexHelper vh, float centerX, float halfBase, float halfTip, float yMin, float yMax, Color32 color32)
    {
        int start = vh.currentVertCount;

        vh.AddVert(new Vector3(centerX - halfBase, yMin), color32, new Vector2(0f, 0f));
        vh.AddVert(new Vector3(centerX - halfTip, yMax), color32, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(centerX + halfTip, yMax), color32, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(centerX + halfBase, yMin), color32, new Vector2(1f, 0f));

        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        raycastTarget = false;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        _widthAtBase = Mathf.Max(0f, _widthAtBase);
        _widthAtTip = Mathf.Max(0f, _widthAtTip);
        _borderWidth = Mathf.Max(0f, _borderWidth);

        UnityEditor.EditorApplication.delayCall -= SyncRectWidth;
        UnityEditor.EditorApplication.delayCall += SyncRectWidth;
    }

    private void SyncRectWidth()
    {
        if (this == null)
            return;

        var (outerHalfBase, outerHalfTip) = OffsetHalfWidths(_widthAtBase * 0.5f, _widthAtTip * 0.5f, rectTransform.rect.height, _borderWidth);
        float width = 2f * Mathf.Max(outerHalfBase, outerHalfTip);

        Vector2 sizeDelta = rectTransform.sizeDelta;

        if (sizeDelta.x == width)
            return;

        sizeDelta.x = width;
        rectTransform.sizeDelta = sizeDelta;
    }
#endif
}
