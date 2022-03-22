using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Library;

[ExecuteAlways]
public class Dot : MonoBehaviour
{
    public string Units
    {
        get
        {
            string result = string.Empty;

            foreach (Unit unit in _units)
                if (unit.enabled)
                    result += (char)unit.Type;

            return result;
        }

        private set
        {
            string result = value.Substring(0, value.Length > 6 ? 6 : value.Length);

            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].enabled = result.Length > i;

                if (_units[i].enabled)
                    _units[i].Type = (Type)result[i];
            }
        }
    }

    public bool Interactable { get => _button.interactable; private set => _button.interactable = value; }
    public Player Player { get => player; private set => player = value; }
    public Type Type { get => type; }
    public int LifeTime { get; private set; } = 0;
    public Vector2 Position 
    { 
        get 
        { 
            if (_rect == null) 
                _rect = GetComponent<RectTransform>(); 
            
            return _rect.anchoredPosition; 
        } 
    }

    [SerializeField]
    private Player player;
    [SerializeField]
    private Type type;
    [SerializeField]
    private string initialUnits = string.Empty;

    [Header("References")]
    [SerializeField]
    private Image _substrate;
    [SerializeField]
    private Image _icon;
    [SerializeField]
    private GameObject _container;

    private Image _circle;
    private Button _button;
    private RectTransform _rect;
    private TurnManager _dotContainer;
    private Unit[] _units;

    public Unit PullUnitOut(char type)
    {
        int index = Units.IndexOf(type);

        if (index < 0)
            return null;

        // create copy of unit at middle of dot
        GameObject obj = Instantiate(_units[index].gameObject, _dotContainer.transform);

        Unit unit = obj.GetComponent<Unit>();
        unit.Type = _units[index].Type;
        unit.Color = Settings.GetColor(Player);
        unit.Position = Position;

        Units = Units.Remove(index, 1);
        return unit;
    }

    public void DoStep()
    {
        if (Player != Player.None && type != Type.None)
            LifeTime++;

        if (_substrate.gameObject.activeSelf)
            _substrate.fillAmount = 
                LifeTime == 0 ? 1f : 1f * LifeTime / Settings.DotSpawnCooldown;

        if (LifeTime == Settings.DotSpawnCooldown)
        {
            Units += (char)type;
            LifeTime = 0;
        }
    }

    public void Invade(char type, Player invader)
    {
        if (Player == invader)
        {
            // just union friendly units
            Units += type;
            return;
        }

        bool result = FightRules.CanCharSurviveSingleAttack(type, Units, out string removal);

        foreach (char symbol in removal)
            Units = Units.Remove(Units.IndexOf(symbol), 1);

        if (!result)
            return;

        // is conquered if invader survives
        Units += type;
        Player = invader;
        LifeTime = 0;
    }

    public bool CanBeReached(Dot other)
    {
        return (other.Position - Position).magnitude <= Settings.MaxReachableDistanceBetweenDots;
    }

    public void OnClick()
    {
        _dotContainer.SelectDot(this);
    }

    private void Awake()
    {
        _circle = GetComponent<Image>();
        _button = GetComponent<Button>();
        _dotContainer = GetComponentInParent<TurnManager>();
        _units = _container.GetComponentsInChildren<Unit>();
        SetDefault();
    }

    private void Update()
    {
        if (Type == Type.None && Units == string.Empty)
            Player = Player.None;

        Interactable = _dotContainer.IsPlayerTurn && _dotContainer.CanDotBePressed(this);
        UpdateColors();

        if (!Application.IsPlaying(gameObject))
            SetDefault();
    }

    private void UpdateColors()
    {
        // get player color
        Color color = Settings.GetColor(Player);
        color.a = 1f;

        // units color
        foreach (Unit unit in _units)
            if (unit.enabled)
                unit.Color = color;

        // main color
        if (Application.IsPlaying(gameObject))
            if (!Interactable)
                color /= 1.5f;

        color.a = 1f;
        _circle.color = color;

        // cooldown image color
        _substrate.color = color;
    }

    private void FixedUpdate()
    {
        _container.transform.Rotate(
            (Player == Player.Green ? 1f : -1f) * Settings.DotSpinSpeed * Time.fixedDeltaTime * Vector3.forward);
    }

    private void SetDefault()
    {
        Units = initialUnits;
        _substrate.gameObject.SetActive(Type != Type.None);
        _icon.sprite = Settings.GetIcon(Type);
    }
}