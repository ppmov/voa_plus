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
    public TrailRenderer Trail { get => _trail; }

    private Image _element;
    private TrailRenderer _trail;
    private RectTransform _rect;
    private Vector3 initialLocalEulerAngles = Vector3.zero;

    public void LookAt(Vector2 target) => _rect.Rotate(Vector3.forward * Vector2.SignedAngle(_rect.up, target - Position));

    private void Start()
    {
        TryFillReferences();
    }

    private void TryFillReferences()
    {
        if (_element == null)
            _element = GetComponent<Image>();

        if (_trail == null)
            _trail = GetComponent<TrailRenderer>();

        if (_rect == null)
            _rect = GetComponent<RectTransform>();

        if (initialLocalEulerAngles == Vector3.zero)
            initialLocalEulerAngles = _rect.localEulerAngles;
    }

    private void OnEnable()
    {
        TryFillReferences();
        _element.enabled = _trail.enabled = true;
    }

    private void OnDisable()
    {
        TryFillReferences();
        Type = Type.None;
        _element.enabled = _trail.enabled = false;
    }

    private void Update()
    {
        if (Settings.GetIcon(Type) != null)
            _element.sprite = Settings.GetIcon(Type);
    }

    public void SetClockwise(int sign)
    {
        if (_rect == null)
            return;

        if (initialLocalEulerAngles == Vector3.zero)
            return;

        _rect.localScale = new Vector3(_rect.localScale.x, sign, _rect.localScale.z);

        if (Application.IsPlaying(gameObject))
            _rect.localEulerAngles = initialLocalEulerAngles + new Vector3(0, 0, -10 * sign);
    }
}
