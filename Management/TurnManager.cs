using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Library;

public class TurnManager : MonoBehaviour, IDotContainer
{
    public Dot[] Dots
    {
        get
        {
            if (_dots != null && _dots.Length > 0)
                return _dots;

            return _dots = GetComponentsInChildren<Dot>();
        }
    }

    public Dot Selected { get; private set; }
    public bool IsPlayerTurn { get => Player == Player.Green; }

    private Player Player { get; set; } = Player.Green;
    private Turn Turn { get; set; } = Turn.Start;
    private bool IsMoving { get; set; } = false;

    private Dot[] _dots;

    public void SelectDot(Dot dot)
    {
        if (IsMoving || dot == null)
            return;

        if (Turn == Turn.Attack)
        {
            StartCoroutine(Move(Selected, dot));
            return;
        }

        Selected = Turn == Turn.Start ? dot : null;
        SwitchTurn();
    }

    public void CancelSelect()
    {
        if (IsMoving || Selected == null)
            return;

        Turn = Turn.Start;
        Selected = null;
    }

    public bool CanDotBePressed(Dot dot, Player lead = Player.None)
    {
        if (Turn == Turn.Start)
            // the start turn dot should has units and must be owned by current player
            return (lead == Player.None ? Player : lead) == dot.Player && dot.Units.Length > 0;
        else
            // the attack turn dot must be at a sufficient distance from the previously selected dot
            return Selected != null && dot != Selected && (dot.Position - Selected.Position).magnitude <= Settings.MaxReachableDistanceBetweenDots;
    }

    private void Start()
    {
        if (!HasAvailables(Player))
        {
            // skip first turn if green player has no units
            Turn = Turn.Attack;
            SwitchTurn();
            return;
        }
    }

    private void SwitchTurn()
    {
        // end game if last player can't do nothing
        if (Turn == Turn.Attack)
            if (!HasAvailables(Player, true))
            {
                EndGame();
                return;
            }

        // switch turn variables
        switch (Player)
        {
            case Player.Green:
                Player = Turn == Turn.Start ? Player.Green : Player.Red;
                Turn = Turn == Turn.Start ? Turn.Attack : Turn.Start;
                break;
            case Player.Red:
                Player = Turn == Turn.Start ? Player.Red : Player.Green;
                Turn = Turn == Turn.Start ? Turn.Attack : Turn.Start;
                break;
        }

        // change player logic
        if (Turn == Turn.Start)
        {
            Selected = null;

            foreach (Dot dot in Dots)
                dot.DoStep();

            // skip turn if next player hasn't got units but has typed dots
            if (!HasAvailables(Player))
            {
                Turn = Turn.Attack;
                SwitchTurn();
                return;
            }
        }

        // AI logic
        if (Player == Player.Red)
            AILogic();
    }

    private bool HasAvailables(Player player, bool typed = false)
    {
        foreach (Dot dot in Dots)
            if (dot.Player == player) 
                if (dot.Units.Length > 0 || (typed && dot.Type != Type.None))
                    return true;

        return false;
    }

    private void EndGame()
    {
        // lose if other player has way
        Player other = Player == Player.Green ? Player.Red : Player.Green;

        foreach (Dot dot in Dots)
            if (HasAvailables(other, true))
            {
                LevelManager.EndGame(Player);
                return;
            }

        // draw
        LevelManager.EndGame(Player.None);
    }

    private IEnumerator Move(Dot from, Dot to)
    {
        IsMoving = true;
        Player owner = from.Player; // player could be changed during the attack
        int maxFixedUpdates = Settings.UnitMoveUpdatesCount;

        // until all symbols won't fly away
        while (from.Units.Length > 0)
        {
            // find best symbol
            char type;

            if (FightRules.HasToAttack(from, to))
                type = FightRules.GetNextUnitToAttack(from.Units, to.Units);
            else
                type = from.Units[0];

            // move it to the middle of dot
            Unit unit = from.PullUnitOut(type);
            unit.LookAt(to.Position);

            // move symbol to target dot
            for (float i = 1; i <= maxFixedUpdates; i++)
            {
                unit.Position = Vector2.Lerp(from.Position, to.Position, i / maxFixedUpdates);
                yield return new WaitForFixedUpdate();
            }

            maxFixedUpdates -= maxFixedUpdates / Settings.UnitMoveCountKf;
            yield return new WaitForFixedUpdate();

            // attack dot if needed
            to.Invade(type, owner);
            Destroy(unit.gameObject);
        }

        yield return new WaitForSeconds(0.3f);
        IsMoving = false;
        SwitchTurn();
    }

    private void AILogic()
    {
        if (Player != Player.Red)
            return;

        switch (Turn)
        {
            // select friendly target dot
            case Turn.Start:
                for (int i = 1; i <= 5; i++)
                    foreach (Dot dot in Dots)
                        if (CanDotBePressed(dot))
                            if (FindBestNeighbor(dot, i) != null)
                            {
                                SelectDot(dot);
                                return;
                            }

                // no way
                SelectDot(null);
                return;

            // select neighbor target dot
            case Turn.Attack:
                SelectDot(FindBestNeighbor(Selected));
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
        foreach (Dot dot in Dots)
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
        int dotCount = 0;

        foreach (Dot dot in Dots)
            if (dot.Type != Type.None)
            {
                if (relative.Player == dot.Player)
                    myDotCount++;
                else
                    dotCount++;
            }

        bool isAgressive = myDotCount <= dotCount && dotCount > 0;

        foreach (Dot dot in Dots)
            if (dot != relative)
                if (dot.Player != relative.Player && (isAgressive || dot.Player != Player.None))
                    if (dot.Type == Type.None)
                        if (dot.CanBeReached(relative))
                            if (FightRules.IsAttackSuccess(relative.Units, dot.Units))
                                return dot;

        if (priority == 2)
            return null;

Defend: // at third it's better to defend ally typed dots
        foreach (Dot dot in Dots)
            if (dot != relative)
                if (dot.Player == relative.Player)
                    if (dot.Type != Type.None)
                        if (dot.CanBeReached(relative))
                            if (dot.Units.Length + relative.Units.Length <= 6)
                                return dot;

        if (priority == 3)
            return null;

Friend: // at forth it's better to combine with other ally
        foreach (Dot dot in Dots)
            if (dot != relative)
                if (dot.Player == relative.Player)
                    if (dot.CanBeReached(relative))
                        if (dot.Units.Length + relative.Units.Length <= 6)
                            return dot;

        if (priority == 4)
            return null;

Anyone: // return any available dot
        foreach (Dot dot in Dots)
            if (dot != relative)
                if (dot.CanBeReached(relative))
                    return dot;

        return null;
    }
}
