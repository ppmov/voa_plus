using UnityEngine;
using Library;

public class Board : MonoBehaviour, IDotContainer
{
    private TurnSwitcher selector;
    private Dot[] dots;

    public Dot[] Dots
    {
        get
        {
            if (dots != null && dots.Length > 0)
                return dots;

            return dots = GetComponentsInChildren<Dot>();
        }
    }

    public void SelectDot(Dot selected)
    {
        for (int i = 0; i < Dots.Length; i++)
            if (selected == Dots[i])
            {
                selector.SelectDot(i);
                return;
            }
    }

    private void Start() => selector = GetComponentInParent<TurnSwitcher>();

    private void Update()
    {
        // update interactable states
        foreach (Dot dot in Dots)
            dot.Interactable = selector.IsMyTurnNow && selector.CanDotBePressed(dot);
    }
}
