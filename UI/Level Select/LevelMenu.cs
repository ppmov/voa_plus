using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    [SerializeField]
    private Vector2 maxLevelPositions;
    [SerializeField]
    private Transform levelContainer;
    [SerializeField]
    private GameObject levelButtonPrefab;

    private void Awake()
    {
        for (int i = 0; i < LevelManager.GetLevelCount(); i++)
        {
            LevelButton button = Instantiate(levelButtonPrefab, levelContainer).GetComponent<LevelButton>();
            button.Level = i;

            int row = i / 3;
            button.Position = new Vector2(maxLevelPositions.x * (i - 1 - 3 * row), 
                                          maxLevelPositions.y - (maxLevelPositions.y * 2 / 3) * row);
        }
    }

    public void LoadLevel(int id)
    {
        LevelManager.LoadLevel(id);
    }
}
