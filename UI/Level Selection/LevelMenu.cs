using Library;
using UnityEngine;

public class LevelMenu : MonoBehaviour
{
    [SerializeField]
    private LevelManager levelManager;
    [SerializeField]
    private Vector2 maxLevelPositions;
    [SerializeField]
    private Transform levelContainer;
    [SerializeField]
    private GameObject levelButtonPrefab;

    private void Awake()
    {
        for (int i = 0; i < levelManager.GetLevelCount(); i++)
        {
            LevelButton button = Instantiate(levelButtonPrefab, levelContainer).GetComponent<LevelButton>();
            button.SetLevel(i);

            int row = i / 3;
            button.Position = new Vector2(maxLevelPositions.x * (i - 1 - 3 * row), 
                                          maxLevelPositions.y - (maxLevelPositions.y * 2 / 3) * row);
        }
    }

    public void LoadLevel(int id) => levelManager.Load(id);

    public IDotContainer GetLevelContainer(int level) => levelManager.GetLevelPrefab(level).GetComponent<IDotContainer>();
}
