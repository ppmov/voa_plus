using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Library;

[ExecuteAlways]
public class Unit : MonoBehaviour
{
    public Type Type { get; set; }
    public Color Color { get => _trail.startColor; set => _trail.startColor = _trail.endColor = value; }
    public Vector2 Position { get => _rect.anchoredPosition; set => _rect.anchoredPosition = value; }
    public Vector2 Direction { get => _rect.up; }
    public Vector2 Size { get => _rect.localScale; set => _rect.localScale = new Vector3(value.x, value.y, 1f); }
    public TrailRenderer Trail { get => _trail; }

    private Image _element;
    private TrailRenderer _trail;
    private RectTransform _rect;

    public void LookAt(Vector2 target) => _rect.Rotate(Vector3.forward * Vector2.SignedAngle(_rect.up, target - Position));

    private void Start()
    { 
        if (_element == null)
            _element = GetComponent<Image>();

        if (_trail == null)
            _trail = GetComponent<TrailRenderer>();

        if (_rect == null)
            _rect = GetComponent<RectTransform>();

        _element.color = Color.black;
    }

    private void OnEnable()
    {
        Start();
        _element.enabled = _trail.enabled = true;
    }

    private void OnDisable()
    {
        Start();
        Type = Type.None;
        _element.enabled = _trail.enabled = false;
    }

    private void Update()
    {
        if (Settings.GetIcon(Type) != null)
            _element.sprite = Settings.GetIcon(Type);
    }
}
