using UnityEngine;
using UnityEngine.UI;
using Library;

public class LevelButton : MonoBehaviour, IDotContainer
{
    public Dot[] Dots
    {
        get
        {
            if (dots != null)
                return dots;

            if (level == -1)
                return null;

            return dots = menu.GetLevelContainer(level).Dots;
        }
    }

    public Vector2 Position { get => rect.anchoredPosition; set => rect.anchoredPosition = value; }

    private int level = -1;
    private RectTransform rect;
    private Button button;
    private LevelMenu menu;
    private Dot[] dots;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        menu = transform.parent.GetComponentInParent<LevelMenu>();
    }

    private void OnEnable()
    {
        if (level == -1)
            return;

        if (Storage.IsLevelCompleted(level))
            button.interactable = true;
        else
        if (level == 0)
            button.interactable = true;
        else
        if (Storage.IsLevelCompleted(level - 1))
            button.interactable = true;
        else
            button.interactable = false;
    }

    public void OnClick()
    {
        menu.LoadLevel(level);
    }

    public void SetLevel(int level)
    {
        this.level = level;
        dots = null;
        OnEnable();
    }
}
