using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ʃh���b�O���W�߁A�q�b�g���� SlicableSprite2D �ɐؒf���˗�����ȈՃR���g���[���B
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class SliceInput2D : MonoBehaviour
{
    public Camera cam;
    public float minStep = 0.01f; // �_�ԋ����̍ŏ��������i���[���h�j

    LineRenderer lr;
    List<Vector2> stroke = new List<Vector2>();
    bool dragging;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            stroke.Clear();
            lr.positionCount = 0;
        }

        if (dragging && Input.GetMouseButton(0))
        {
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -cam.transform.position.z));
            w.z = 0f;
            if (stroke.Count == 0 || (stroke[^1] - (Vector2)w).sqrMagnitude > minStep * minStep)
            {
                stroke.Add(w);
                lr.positionCount = stroke.Count;
                lr.SetPosition(stroke.Count - 1, w);
            }
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            TryApplySlice(stroke);
            lr.positionCount = 0;
        }
    }

    void TryApplySlice(List<Vector2> strokeWorld)
    {
        if (strokeWorld.Count < 2) return;

        // ���ƌ����������Ȍ�������������iAABB�Łj
        var slicables = GameObject.FindObjectsOfType<SlicableSprite2D>();
        foreach (var s in slicables)
        {
            var b = s.GetComponent<SpriteRenderer>().bounds;
            if (!PolylineIntersectsAABB(strokeWorld, b)) continue;

            if (s.TrySliceByStroke(strokeWorld))
            {
                // �����X���C�v�ŕ����؂肽����� continue
                // �Ƃ肠����1�ł��؂ꂽ��OK
                // break;
            }
        }
    }

    static bool PolylineIntersectsAABB(List<Vector2> pl, Bounds b)
    {
        for (int i = 1; i < pl.Count; i++)
        {
            if (SegmentAABB(pl[i - 1], pl[i], b)) return true;
        }
        return false;
    }

    static bool SegmentAABB(Vector2 a, Vector2 b, Bounds bb)
    {
        // Liang?Barsky �ȈՔłł��ǂ����A�����ł͑e���`�F�b�N
        var min = (Vector2)bb.min; var max = (Vector2)bb.max;
        // �����ے�i���[�����S�ɕБ��j
        if (Mathf.Max(a.x, b.x) < min.x || Mathf.Min(a.x, b.x) > max.x ||
            Mathf.Max(a.y, b.y) < min.y || Mathf.Min(a.y, b.y) > max.y) return false;
        return true;
    }
}
