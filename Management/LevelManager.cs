using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Library;

public class LevelManager : MonoBehaviour
{
    public static bool IsLevelLoaded { get; private set; } = false;
    public static int CurrentLevel { get => _instance._currentLevel; }

    [SerializeField]
    private Transform desk;
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private Button nextButton;
    [SerializeField]
    private RulesWindow rules;

    private static LevelManager _instance;
    private GameObject[] _levels;
    private TurnManager _container;
    private int _currentLevel = 0;

    public static List<Type> GetCurrentLevelDotTypes()
    {
        List<Type> types = new List<Type>();

        foreach (Dot dot in _instance._container.Dots)
            if (dot.Type != Type.None)
                if (!types.Contains(dot.Type))
                    types.Add(dot.Type);

        foreach (Dot dot in _instance._container.Dots)
            foreach (char unit in dot.Units)
                if (!types.Contains((Type)unit))
                    types.Add((Type)unit);

        return types;
    }

    public static int GetLevelCount()
    {
        return _instance._levels.Length;
    }

    public static void LoadLevel(int id)
    {
        if (id < 0 || id >= GetLevelCount())
            return;

        if (_instance._container != null)
            Destroy(_instance._container.gameObject);
        
        GameObject level = Instantiate(_instance._levels[id], _instance.desk);
        _instance._container = level.GetComponent<TurnManager>();

        _instance._currentLevel = id;
        _instance.menu.SetActive(false);
        _instance.nextButton.gameObject.SetActive(false);
        _instance.rules.RespawnUnits();

        IsLevelLoaded = true;
    }

    public static void EndGame(Player loser)
    {
        string result = loser switch
        {
            Player.None => "Draw",
            Player.Green => "Lose",
            Player.Red => "Win",
            _ => string.Empty
        };

        _instance.nextButton.gameObject.SetActive(true);
        _instance.nextButton.GetComponentInChildren<Text>().text = result;

        IsLevelLoaded = false;
    }

    public void OnRestart()
    {
        LoadLevel(_currentLevel);
    }

    public void OnMenu()
    {
        menu.SetActive(true);
        nextButton.gameObject.SetActive(false);
    }

    public void OnClose()
    {
        menu.SetActive(false);
    }

    public void OnNext()
    {
        if (nextButton.GetComponentInChildren<Text>().text != "Win")
            LoadLevel(_currentLevel);
        else
        if (_currentLevel + 1 >= _levels.Length)
            OnMenu();
        else
            LoadLevel(_currentLevel + 1);
    }

    private void Awake()
    {
        _instance = this;
        _levels = Resources.LoadAll<GameObject>("Boards");
    }

    private void Start()
    {
        LoadLevel(_currentLevel);
    }
}
