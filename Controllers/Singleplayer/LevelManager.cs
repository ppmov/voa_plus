using Library;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private Transform desk;

    private GameObject[] levels;

    public int CurrentLevel { get; private set; } = 0;

    public UnityEvent<Board> OnLevelLoaded;
    public UnityEvent OnNothingToLoadNext;

    public int GetLevelCount() => levels.Length;

    public GameObject GetLevelPrefab(int index) => levels[index];

    public void Load(int id)
    {
        if (id < 0 || id >= GetLevelCount())
            return;

        // create new desk
        CurrentLevel = id;
        Board board = Instantiate(levels[id], desk).GetComponent<Board>();
        StartCoroutine(LateLoad(board));
    }

    private IEnumerator LateLoad(Board board)
    {
        yield return new WaitForEndOfFrame();
        OnLevelLoaded.Invoke(board);
    }

    public void LoadNext()
    {
        if (!Storage.IsLevelCompleted(CurrentLevel))
            return;

        if (CurrentLevel + 1 >= GetLevelCount())
            OnNothingToLoadNext.Invoke();
        else
            Load(CurrentLevel + 1);
    }

    public void Restart()
    {
        Load(CurrentLevel);
    }

    public void DeclareDefeat(Player loser)
    {
        if (loser == Player.Red)
            Storage.CompleteLevel(CurrentLevel);
    }

    private void Awake()
    {
        GameObject[] trainings = Resources.LoadAll<GameObject>("Boards/Training");
        GameObject[] balanced = Resources.LoadAll<GameObject>("Boards/Balanced");

        levels = new GameObject[trainings.Length + balanced.Length];

        for (int i = 0; i < trainings.Length; i++)
            levels[i] = trainings[i];

        for (int i = 0; i < balanced.Length; i++)
            levels[i + trainings.Length] = balanced[i];
    }

    private void Start()
    {
        for (int i = 0; i < levels.Length; i++)
            if (!Storage.IsLevelCompleted(i))
            {
                Load(i);
                return;
            }

        OnNothingToLoadNext.Invoke();
    }
}
