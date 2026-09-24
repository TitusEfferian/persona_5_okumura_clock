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

    [Tooltip("Draw the RectTransform's rect behind the hand, to see the width that SyncRectWidth keeps in step with the corners.")]
    [SerializeField]
    private bool _showRect = false;

    [Tooltip("Colour of the rect fill. Alpha below 1 lets the hand and whatever is behind it show through.")]
    [SerializeField]
    private Color _rectColor = new Color(1f, 1f, 1f, 0.25f);

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

    public bool ShowRect
    {
        get => _showRect;
        set
        {
            if (_showRect == value)
                return;

            _showRect = value;
            SetVerticesDirty();
        }
    }

    public Color RectColor
    {
        get => _rectColor;
        set
        {
            if (_rectColor == value)
                return;

            _rectColor = value;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();

        if (rect.height <= 0f)
            return;

        if (_showRect)
            AddQuad(vh, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), _rectColor);

        var (bottomLeft, topLeft, topRight, bottomRight) = GetCorners(rect.center.x, rect.yMin, rect.height);

        AddQuad(vh, bottomLeft, topLeft, topRight, bottomRight, color);
    }

    private (Vector2, Vector2, Vector2, Vector2) GetCorners(float centerX, float yMin, float height)
    {
        float halfBase = _widthAtBase * 0.5f;
        float halfTip = _widthAtTip * 0.5f;
        float slope = height > 0f ? (halfTip - halfBase) / height : 0f;
        float tangentBase = SkewTangent(_skewAtBase, slope);
        float tangentTip = SkewTangent(_skewAtTip, slope);
        float yMax = yMin + height;

        return (
            Corner(centerX, yMin, -halfBase, -slope, tangentBase),
            Corner(centerX, yMax, -halfTip, -slope, tangentTip),
            Corner(centerX, yMax, halfTip, slope, tangentTip),
            Corner(centerX, yMin, halfBase, slope, tangentBase));
    }

    private static float SkewTangent(float skew, float slope)
    {
        float tangent = Mathf.Tan(skew * Mathf.Deg2Rad);

        return Mathf.Abs(slope * tangent) < 1f ? tangent : 0f;
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

        UnityEditor.EditorApplication.delayCall -= SyncRectWidth;
        UnityEditor.EditorApplication.delayCall += SyncRectWidth;
    }

    private void SyncRectWidth()
    {
        if (this == null)
            return;

        var (bottomLeft, topLeft, topRight, bottomRight) = GetCorners(0f, 0f, rectTransform.rect.height);
        float width = 2f * Mathf.Max(Mathf.Abs(bottomLeft.x), Mathf.Abs(topLeft.x), Mathf.Abs(topRight.x), Mathf.Abs(bottomRight.x));

        Vector2 sizeDelta = rectTransform.sizeDelta;

        if (sizeDelta.x == width)
            return;

        sizeDelta.x = width;
        rectTransform.sizeDelta = sizeDelta;
    }
#endif
}
