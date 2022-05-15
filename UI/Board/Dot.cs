using UnityEngine;
using UnityEngine.UI;
using Library;
using System.Collections.Generic;

[ExecuteAlways]
public class Dot : MonoBehaviour
{
    [Header("Options")]
    [SerializeField]
    private Player player;
    [SerializeField]
    private Type type;
    [SerializeField]
    private string initialUnits = string.Empty;

    [Header("References")]
    [SerializeField]
    private Image substrate;
    [SerializeField]
    private GameObject unitContainer;
    [SerializeField]
    private List<Image> colorUpdatedImages;
    [SerializeField]
    private List<Image> spriteUpdatedImages;

    private Button button;
    private RectTransform _rect;
    private Board board;
    private Unit[] units;

    public string Units
    {
        get
        {
            if (units == null)
                units = unitContainer.GetComponentsInChildren<Unit>();

            string result = string.Empty;

            foreach (Unit unit in units)
                if (unit.enabled)
                    result += (char)unit.Type;

            return result;
        }

        private set
        {
            if (units == null)
                units = unitContainer.GetComponentsInChildren<Unit>();

            string result = value.Substring(0, value.Length > units.Length ? units.Length : value.Length);

            for (int i = 0; i < units.Length; i++)
            {
                units[i].enabled = result.Length > i;

                if (units[i].enabled)
                    units[i].Type = (Type)result[i];
            }

            UpdateUnitVisual();
        }
    }

    public Player Player
    {
        get => player;
        private set
        {
            player = value;
            UpdateUnitVisual();
        }
    }

    public bool Interactable
    {
        get => button.interactable;
        set
        {
            if (!Application.IsPlaying(gameObject))
                return;

            button.interactable = value;
            UpdateUnitVisual();
        }
    }

    public Vector2 Position
    {
        get
        {
            if (_rect == null)
                _rect = GetComponent<RectTransform>();

            return _rect.anchoredPosition;
        }
    }

    public Type Type { get => type; }
    public int LifeTime { get; private set; } = 0;

    private int RotateSign => Player == Player.Green ? 1 : -1;

    public bool CanBeReached(Dot other) => (other.Position - Position).magnitude <= Settings.MaxReachableDistanceBetweenDots;

    public void OnClick() => board.SelectDot(this);

    public Unit PullUnitOut(char type)
    {
        int index = Units.IndexOf(type);

        if (index < 0)
            return null;

        // create copy of unit at middle of dot
        GameObject obj = Instantiate(units[index].gameObject, board.transform);

        Unit unit = obj.GetComponent<Unit>();
        unit.Type = units[index].Type;
        unit.Color = Settings.GetColor(Player);
        unit.Position = Position;

        Units = Units.Remove(index, 1);
        return unit;
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

    public void Free()
    {
        Player = Player.None;
        Units = string.Empty;
        LifeTime = 0;
    }

    public void DoStep()
    {
        if (Player != Player.None && Type != Type.None)
            LifeTime++;

        if (substrate.gameObject.activeSelf)
            substrate.fillAmount =
                LifeTime == 0 ? 1f : 1f * LifeTime / Settings.DotSpawnCooldown;

        if (LifeTime == Settings.DotSpawnCooldown)
        {
            Units += (char)Type;
            LifeTime = 0;
        }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        board = GetComponentInParent<Board>();
        units = unitContainer.GetComponentsInChildren<Unit>();
    }

    private void Start() => SetDefault();

    private void Update()
    {
        if (Type == Type.None && Units == string.Empty && Player != Player.None)
            Free();

        if (!Application.IsPlaying(gameObject))
            SetDefault();
    }

    private void FixedUpdate()
    {
        unitContainer.transform.Rotate(RotateSign * Settings.DotSpinSpeed * Time.fixedDeltaTime * Vector3.forward);
    }

    private void SetDefault()
    {
        Units = initialUnits;
        substrate.gameObject.SetActive(Type != Type.None);

        foreach (Image image in spriteUpdatedImages)
        {
            image.gameObject.SetActive(Type != Type.None);
            image.sprite = Settings.GetIcon(Type);
        }
    }

    private void UpdateUnitVisual()
    {
        Color color = Settings.GetColor(Player);
        color.a = 1f;

        // units color
        foreach (Unit unit in units)
            if (unit.enabled)
                unit.Color = color;

        // main color
        if (Application.IsPlaying(gameObject))
            if (!Interactable)
                color /= 1.5f;

        color.a = 1f;

        foreach (Image image in colorUpdatedImages)
            image.color = color;

        // invert unit depended on dot rotation
        foreach (Unit unit in units)
            unit.SetClockwise(RotateSign);
    }
}