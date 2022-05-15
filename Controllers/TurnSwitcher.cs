using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Library;

public class TurnSwitcher: MonoBehaviour, IDotContainer
{
    public Dot[] Dots { get => dotContainer.Dots; }

    public Dot Selected { get; private set; }
    public virtual bool IsMyTurnNow { get => Player == Player.Green; }

    public Player Player { get; private set; } = Player.Green;
    public Turn Turn { get; private set; } = Turn.Start;
    public bool IsMoving { get; private set; } = false;

    public UnityEvent OnTurnSwitched;
    public UnityEvent<Player> OnDeadEnd;

    private Board dotContainer;

    public void StartGame(Board board)
    {
        if (dotContainer != null)
            Destroy(dotContainer.gameObject);

        dotContainer = board;
        Player = Player.Green;
        Turn = Turn.Start;
        Selected = null;
        IsMoving = false;
        StopAllCoroutines();

        if (!HasAvailables(Player))
            // skip first turn if green player has no units
            SkipTurn();
        else
            OnTurnSwitched.Invoke();
    }

    public void SelectDot(int index) => SelectDot(Dots[index]);

    public virtual void SelectDot(Dot dot)
    {
        if (IsMoving || dot == null)
            return;

        if (Turn == Turn.Attack && dot == Selected)
        {
            CancelSelect();
            return;
        }

        if (Turn == Turn.Attack)
        {
            StartCoroutine(Move(Selected, dot));
            return;
        }

        Selected = Turn == Turn.Start ? dot : null;
        SwitchTurn();
    }

    public virtual void CancelSelect()
    {
        if (IsMoving || Selected == null)
            return;

        Turn = Turn.Start;
        Selected = null;
        OnTurnSwitched.Invoke();
    }

    public bool CanDotBePressed(Dot dot)
    {
        if (Turn == Turn.Start)
            // the start turn dot should has units and must be owned by current player
            return Player == dot.Player && dot.Units.Length > 0;
        else
            // the attack turn dot must be at a sufficient distance from the previously selected dot
            return Selected != null && (dot.Position - Selected.Position).magnitude <= Settings.MaxReachableDistanceBetweenDots; //&& dot != Selected
    }

    public List<Type> GetCurrentLevelDotTypes()
    {
        List<Type> types = new List<Type>();

        foreach (Dot dot in Dots)
            if (dot.Type != Type.None)
                if (!types.Contains(dot.Type))
                    types.Add(dot.Type);

        foreach (Dot dot in Dots)
            foreach (char unit in dot.Units)
                if (!types.Contains((Type)unit))
                    types.Add((Type)unit);

        return types;
    }

    private void SkipTurn()
    {
        Turn = Turn.Attack;
        SwitchTurn();
    }

    private void SwitchTurn()
    {
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

            // end game if any player can't do nothing
            if (!HasAvailables(Player.Green, true) || !HasAvailables(Player.Red, true))
                EndGame();
            else
            // skip turn if next player hasn't got units but has typed dots
            if (!HasAvailables(Player))
                SkipTurn();
        }

        OnTurnSwitched.Invoke();
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
        if (!HasAvailables(Player.Green, true) && !HasAvailables(Player.Red, true))
        {
            OnDeadEnd.Invoke(Player.None);
            return;
        }

        if (!HasAvailables(Player.Green, true))
            OnDeadEnd.Invoke(Player.Green);
        else
        if (!HasAvailables(Player.Red, true))
            OnDeadEnd.Invoke(Player.Red);
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
}
