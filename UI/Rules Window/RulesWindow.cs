using System.Collections.Generic;
using UnityEngine;
using Library;

public class RulesWindow : MonoBehaviour
{
    private enum Phase { Nothing, Destroy, Pause }

    [SerializeField]
    private RectTransform container;
    [SerializeField]
    private GameObject _element;
    [SerializeField]
    private GameObject _operator;
    [SerializeField]
    private float _moveSpeed = 50f;
    [SerializeField]
    private Vector2 _maxSpawnPosition;
    [SerializeField]
    private TurnSwitcher _turnManager;

    private Phase Current { get; set; } = Phase.Nothing;
    private List<Unit> Units { get; set; } = new List<Unit>(6);
    private List<Unit> Stopped { get; set; } = new List<Unit>();
    private List<RectTransform> Operators { get; set; } = new List<RectTransform>(3);
    private List<Vector2> SpawnPositions { get; set; } = new List<Vector2>(6);

    private float initialTrailTime = 0;
    private float DestroyPlace { get => 0.3f * _maxSpawnPosition.x; }
    private float PausePlace { get => 0.4f * _maxSpawnPosition.x; }

    public void RespawnUnits()
    {
        ClearAll();

        List<Type> types = new List<Type>()
        {
            Type.Paper,
            Type.Rock,
            Type.Scissors
        };

        // draw unit fight if there are different typed dots or units on a level
        types = _turnManager.GetCurrentLevelDotTypes();

        int j = 0;

        for (int i = 0; i < 6; i += 2)
            if (types.Contains(GetType(i)) && types.Contains(FightRules.GetWeakest(GetType(i))))
            {
                Units.Add(CreateUnit(SpawnPositions[j], GetType(i))); // winner
                Units.Add(CreateUnit(SpawnPositions[j + 1], FightRules.GetWeakest(GetType(i)))); // loser
                j += 2;

                if (Current != Phase.Destroy)
                {
                    RectTransform more = Instantiate(_operator, transform).GetComponent<RectTransform>();
                    more.anchoredPosition = new Vector2(0f, Units[j - 1].Position.y);
                    more.gameObject.SetActive(false);
                    Operators.Add(more);
                }
            }

        UpdateColors();
        SwitchPhase();

        static Type GetType(int i) => i switch
        {
            0 => Type.Rock,
            2 => Type.Paper,
            4 => Type.Scissors,
            _ => Type.None,
        };
    }

    private void Awake()
    {
        void AddSpawn(int x, int y) => SpawnPositions.Add(_maxSpawnPosition * new Vector2(x, y));

        // fill spawn positions
        AddSpawn(-1, -1); 
        AddSpawn(1, -1);
        AddSpawn(-1, 0); 
        AddSpawn(1, 0);
        AddSpawn(-1, 1); 
        AddSpawn(1, 1);
    }

    private void Start()
    {
        _turnManager.OnTurnSwitched.AddListener(UpdateColors);
    }

    private void FixedUpdate()
    {
        bool noOneStayed = true;

        // move all available units
        foreach (Unit unit in Units)
        {
            if (unit == null)
                continue;

            if (Stopped.Contains(unit))
                continue;

            noOneStayed = false;
            float scaledMoveSpeed = _moveSpeed;
            
            // strongest units should move faster on first phase
            if (Current == Phase.Pause || unit.Direction.x < 0)
                scaledMoveSpeed /= 2f;

            unit.Position += scaledMoveSpeed * Time.fixedDeltaTime * unit.Direction;
            TryFinishMoving(unit);
        }

        // if no available units left
        if (noOneStayed)
            SwitchPhase();
    }

    private void SwitchPhase()
    {
        switch (Current)
        {
            case Phase.Nothing:
                Current = Units.Count > 0 ? Phase.Destroy : Phase.Nothing;
                break;

            case Phase.Destroy:
                RespawnUnits();
                Current = Phase.Pause;
                break;

            case Phase.Pause:
                foreach (RectTransform rect in Operators)
                    rect.gameObject.SetActive(true);
                break;
        }
    }

    private void UpdateColors()
    {
        Color color = Settings.GetColor(_turnManager.Player);

        foreach (Unit unit in Units)
            if (unit != null)
                unit.Color = unit.Direction.x > 0 ? color : Color.white;

        foreach (Unit unit in Stopped)
            if (unit != null)
                unit.Color = unit.Direction.x > 0 ? color : Color.white;
    }

    private void ClearAll()
    {
        Current = Phase.Nothing;

        foreach (Unit unit in Units)
            if (unit != null)
                Destroy(unit.gameObject);

        foreach (RectTransform rect in Operators)
            if (rect != null)
                Destroy(rect.gameObject);

        Units.Clear();
        Stopped.Clear();
        Operators.Clear();
    }

    private void TryFinishMoving(Unit unit)
    {
        // from left to right direction
        if (unit.Direction.x > 0)
        {
            if (Current == Phase.Destroy)
            {
                if (unit.Position.x >= _maxSpawnPosition.x)
                    Destroy(unit.gameObject);
            }
            else
            if (Current == Phase.Pause)
            {
                if (unit.Position.x >= -PausePlace)
                    Stop(unit);
            }
        }
        // from right to left direction
        else
        {
            if (Current == Phase.Destroy)
            {
                if (unit.Position.x <= DestroyPlace)
                    Destroy(unit.gameObject);
            }
            else
            if (Current == Phase.Pause)
            {
                if (unit.Position.x <= PausePlace)
                    Stop(unit);
            }
        }
    }

    private Unit CreateUnit(Vector2 spawn, Type type)
    {
        Unit unit = Instantiate(_element, container).GetComponent<Unit>();
        unit.Position = spawn;
        unit.LookAt(spawn * new Vector2(-1, 1));
        unit.Trail.time += 0.3f;
        unit.Type = type;

        if (initialTrailTime == 0)
            initialTrailTime = unit.Trail.time;

        return unit;
    }

    private void Stop(Unit unit)
    {
        if (Stopped.Contains(unit))
            return;
        
        Stopped.Add(unit);
        unit.Trail.time = Mathf.Infinity;
    }
}
