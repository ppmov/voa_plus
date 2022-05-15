using UnityEngine;
using Library;
using System.Collections;

public class AIController : MonoBehaviour
{
    [SerializeField]
    private TurnSwitcher manager;
    [SerializeField]
    private float turnDelay;

    private void Start()
    {
        manager.OnTurnSwitched.AddListener(TryDoTurn);
    }

    private void TryDoTurn()
    {
        if (manager.Player != Player.Red || manager.IsMoving)
            return;

        StartCoroutine(WaitAndDoTurn());
    }

    private IEnumerator WaitAndDoTurn()
    {
        yield return new WaitForSeconds(turnDelay);
        DoTurn();
    }

    private void DoTurn()
    {
        switch (manager.Turn)
        {
            // select friendly target dot
            case Turn.Start:
                for (int i = 1; i <= 5; i++)
                    foreach (Dot dot in manager.Dots)
                        if (manager.CanDotBePressed(dot))
                            if (FindBestNeighbor(dot, i) != null)
                            {
                                manager.SelectDot(dot);
                                return;
                            }

                // no way
                manager.SelectDot(null);
                return;

            // select neighbor target dot
            case Turn.Attack:
                manager.SelectDot(FindBestNeighbor(manager.Selected));
                return;
        }
    }

    private Dot FindBestNeighbor(Dot relative, int priority = 0)
    {
        switch (priority)
        {
            case 1:
                goto Typed;
            case 2:
                goto Enemy;
            case 3:
                goto Defend;
            case 4:
                goto Friend;
            case 5:
                goto Anyone;
            default:
                break;
        }

        Typed: // at first it's better to attack typed dot of the weakest enemy
        foreach (Dot dot in manager.Dots)
            if (dot != relative)
                if (dot.Player != relative.Player)
                    if (dot.Type != Type.None)
                        if (dot.CanBeReached(relative))
                            if (FightRules.IsAttackSuccess(relative.Units, dot.Units))
                                return dot;

        if (priority == 1)
            return null;

        Enemy: // at second it's better to attack the weakest enemy
        int myDotCount = 0;
        int otherDotCount = 0;

        foreach (Dot dot in manager.Dots)
            if (dot.Type != Type.None)
            {
                if (relative.Player == dot.Player)
                    myDotCount++;
                else
                    otherDotCount++;
            }

        // enable agressive mode depends on controlled dot count
        bool isAgressive = myDotCount <= otherDotCount && otherDotCount > 0;

        foreach (Dot dot in manager.Dots)
            if (dot != relative)
                if (dot.Player != relative.Player && (isAgressive || dot.Player != Player.None))
                    if (dot.Type == Type.None)
                        if (dot.CanBeReached(relative))
                            if (FightRules.IsAttackSuccess(relative.Units, dot.Units))
                                return dot;

        if (priority == 2)
            return null;

        Defend: // at third it's better to defend ally typed dots
        foreach (Dot dot in manager.Dots)
            if (dot != relative)
                if (dot.Player == relative.Player)
                    if (dot.Type != Type.None)
                        if (dot.CanBeReached(relative))
                            if (dot.Units.Length + relative.Units.Length <= 6)
                                return dot;

        if (priority == 3)
            return null;

        Friend: // at forth it's better to combine with other ally
        foreach (Dot dot in manager.Dots)
            if (dot != relative)
                if (dot.Player == relative.Player)
                    if (dot.CanBeReached(relative))
                        if (dot.Units.Length + relative.Units.Length <= 6)
                            return dot;

        if (priority == 4)
            return null;

        Anyone: // return any available dot
        foreach (Dot dot in manager.Dots)
            if (dot != relative)
                if (dot.CanBeReached(relative))
                    return dot;

        return null;
    }
}
