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

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Color32 color32 = color;

        float halfBase = _widthAtBase * 0.5f;
        float halfTip = _widthAtTip * 0.5f;
        float centerX = rect.center.x;

        int start = vh.currentVertCount;

        vh.AddVert(new Vector3(centerX - halfBase, rect.yMin), color32, new Vector2(0f, 0f));
        vh.AddVert(new Vector3(centerX - halfTip, rect.yMax), color32, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(centerX + halfTip, rect.yMax), color32, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(centerX + halfBase, rect.yMin), color32, new Vector2(1f, 0f));

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

        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.x = Mathf.Max(_widthAtBase, _widthAtTip);
        rectTransform.sizeDelta = sizeDelta;
    }
#endif
}
