using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Library;

public class LevelButton : MonoBehaviour, IDotContainer
{
    public Dot[] Dots
    {
        get
        {
            if (_dots != null)
                return _dots;

            string fileName = (Level + 1).ToString();

            if (Level < 9)
                fileName = '0' + fileName;

            GameObject prefab = Resources.Load<GameObject>("Boards/" + fileName);
            IDotContainer container = prefab.GetComponent<IDotContainer>();
            return _dots = container.Dots;
        }
    }

    public int Level { get; set; }
    public Vector2 Position { get => _rect.anchoredPosition; set => _rect.anchoredPosition = value; }

    private RectTransform _rect;
    private Button _button;
    private LevelMenu _menu;
    private Dot[] _dots;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        _menu = transform.parent.GetComponentInParent<LevelMenu>();
    }

    public void OnClick()
    {
        _menu.LoadLevel(Level);
    }
}
