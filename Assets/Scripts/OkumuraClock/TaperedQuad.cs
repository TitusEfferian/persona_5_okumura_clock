using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Okumura Clock/Tapered Quad")]
[RequireComponent(typeof(CanvasRenderer))]
public class TaperedQuad : MaskableGraphic
{
    private const float MaxSkew = 45f;

    [Tooltip("Width of the hand at the base, in the canvas' reference pixels.")]
    [Min(0f)]
    [SerializeField]
    private float _widthAtBase = 21f;

    [Tooltip("Width of the hand at the tip, in the canvas' reference pixels.")]
    [Min(0f)]
    [SerializeField]
    private float _widthAtTip = 12f;

    [Tooltip("Angle of the cut at the base, in degrees away from square. Positive tilts it counter-clockwise, raising the +X corner.")]
    [Range(-MaxSkew, MaxSkew)]
    [SerializeField]
    private float _skewAtBase = 0f;

    [Tooltip("Angle of the cut at the tip, in degrees away from square. Positive tilts it counter-clockwise, raising the +X corner.")]
    [Range(-MaxSkew, MaxSkew)]
    [SerializeField]
    private float _skewAtTip = 0f;

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

    public float SkewAtBase
    {
        get => _skewAtBase;
        set
        {
            float clamped = Mathf.Clamp(value, -MaxSkew, MaxSkew);

            if (_skewAtBase == clamped)
                return;

            _skewAtBase = clamped;
            SetVerticesDirty();
        }
    }

    public float SkewAtTip
    {
        get => _skewAtTip;
        set
        {
            float clamped = Mathf.Clamp(value, -MaxSkew, MaxSkew);

            if (_skewAtTip == clamped)
                return;

            _skewAtTip = clamped;
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

        if (_borderWidth > 0f)
        {
            var (borderBottomLeft, borderTopLeft, borderTopRight, borderBottomRight) = GetCorners(rect.center.x, rect.yMin, rect.height, _borderWidth);

            Color borderColor = _borderColor;
            borderColor.a *= color.a;

            AddQuad(vh, borderBottomLeft, borderTopLeft, borderTopRight, borderBottomRight, borderColor);
        }

        var (bottomLeft, topLeft, topRight, bottomRight) = GetCorners(rect.center.x, rect.yMin, rect.height, 0f);

        AddQuad(vh, bottomLeft, topLeft, topRight, bottomRight, color);
    }

    private (Vector2, Vector2, Vector2, Vector2) GetCorners(float centerX, float yMin, float height, float offset)
    {
        float halfBase = _widthAtBase * 0.5f;
        float halfTip = _widthAtTip * 0.5f;
        float slope = height > 0f ? (halfTip - halfBase) / height : 0f;
        float tangentBase = SkewTangent(_skewAtBase, slope);
        float tangentTip = SkewTangent(_skewAtTip, slope);
        float secantBase = Mathf.Sqrt(1f + tangentBase * tangentBase);
        float secantTip = Mathf.Sqrt(1f + tangentTip * tangentTip);
        var (outerHalfBase, outerHalfTip) = OffsetHalfWidths(halfBase, halfTip, height, offset, secantBase, secantTip);
        float yMax = yMin + height;
        float yBase = yMin - offset * secantBase;
        float yTip = yMax + offset * secantTip;

        return (
            Corner(centerX, yBase, -outerHalfBase, -slope, tangentBase),
            Corner(centerX, yTip, -outerHalfTip, -slope, tangentTip),
            Corner(centerX, yTip, outerHalfTip, slope, tangentTip),
            Corner(centerX, yBase, outerHalfBase, slope, tangentBase));
    }

    private static float SkewTangent(float skew, float slope)
    {
        float tangent = Mathf.Tan(skew * Mathf.Deg2Rad);

        return Mathf.Abs(slope * tangent) < 1f ? tangent : 0f;
    }

    private static (float, float) OffsetHalfWidths(float halfBase, float halfTip, float height, float offset, float secantBase, float secantTip)
    {
        if (height <= 0f)
            return (halfBase + offset, halfTip + offset);

        float delta = halfTip - halfBase;
        float slant = Mathf.Sqrt(height * height + delta * delta);

        return (halfBase + offset * (slant - delta * secantBase) / height, halfTip + offset * (slant + delta * secantTip) / height);
    }

    private static Vector2 Corner(float centerX, float y, float halfWidth, float slope, float tangent)
    {
        float dx = halfWidth / (1f - slope * tangent);

        return new Vector2(centerX + dx, y + dx * tangent);
    }

    private static void AddQuad(VertexHelper vh, Vector2 bottomLeft, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Color32 color32)
    {
        int start = vh.currentVertCount;

        vh.AddVert(bottomLeft, color32, new Vector2(0f, 0f));
        vh.AddVert(topLeft, color32, new Vector2(0f, 1f));
        vh.AddVert(topRight, color32, new Vector2(1f, 1f));
        vh.AddVert(bottomRight, color32, new Vector2(1f, 0f));

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
        _skewAtBase = Mathf.Clamp(_skewAtBase, -MaxSkew, MaxSkew);
        _skewAtTip = Mathf.Clamp(_skewAtTip, -MaxSkew, MaxSkew);
        _borderWidth = Mathf.Max(0f, _borderWidth);

        UnityEditor.EditorApplication.delayCall -= SyncRectWidth;
        UnityEditor.EditorApplication.delayCall += SyncRectWidth;
    }

    private void SyncRectWidth()
    {
        if (this == null)
            return;

        var (bottomLeft, topLeft, topRight, bottomRight) = GetCorners(0f, 0f, rectTransform.rect.height, _borderWidth);
        float width = 2f * Mathf.Max(Mathf.Abs(bottomLeft.x), Mathf.Abs(topLeft.x), Mathf.Abs(topRight.x), Mathf.Abs(bottomRight.x));

        Vector2 sizeDelta = rectTransform.sizeDelta;

        if (sizeDelta.x == width)
            return;

        sizeDelta.x = width;
        rectTransform.sizeDelta = sizeDelta;
    }
#endif
}
