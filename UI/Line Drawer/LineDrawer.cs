using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Library;

// draw lines between dots
[ExecuteInEditMode]
public class LineDrawer : MonoBehaviour
{
    [SerializeField]
    private float _lineWidth = 5f;
    [SerializeField]
    private GameObject _linePrefab;

    private RectTransform _rect;

    private List<Vector2> DotPositions { get; set; }
    private List<RectTransform> Lines { get; set; }

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        DotPositions = new List<Vector2>();
        IDotContainer container = GetComponentInParent<IDotContainer>();

        if (container != null)
            foreach (Dot dot in container.Dots)
                DotPositions.Add(dot.Position);

        Lines = new List<RectTransform>();

        for (int i = 0; i < DotPositions.Count; i++)
            for (int j = 0; j < DotPositions.Count; j++)
            {
                if (i == j)
                    continue;

                if ((DotPositions[i] - DotPositions[j]).magnitude <= Settings.MaxReachableDistanceBetweenDots)
                    DrawLine((DotPositions[i] + DotPositions[j]) / 2f,
                             Vector2.SignedAngle(Vector2.right, DotPositions[i] - DotPositions[j]));
            }
    }

    private void DrawLine(Vector2 center, float angle)
    {
        foreach (RectTransform line in Lines)
            if (line.anchoredPosition == center)
                return;

        Lines.Add(Instantiate(_linePrefab, 
                              Vector3.zero, 
                              Quaternion.AngleAxis(angle, Vector3.forward), 
                              _rect).GetComponent<RectTransform>());

        Lines[Lines.Count - 1].anchoredPosition = center;
        Lines[Lines.Count - 1].sizeDelta = new Vector2(Lines[Lines.Count - 1].sizeDelta.x, _lineWidth);
    }
}
