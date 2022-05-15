using UnityEngine;
using Library;

public static class FightRules
{
    public static char GetWeakest(char type) => (Type)type switch
    {
        Type.Paper => (char)Type.Rock,
        Type.Rock => (char)Type.Scissors,
        Type.Scissors => (char)Type.Paper,
        _ => (char)Type.None,
    };

    public static Type GetWeakest(Type type) => (Type)GetWeakest((char)type);

    public static bool HasToAttack(Dot from, Dot to) => (from.Player != to.Player && to.Units.Length > 0);

    public static bool IsAttackSuccess(string attacker, string defender)
    {
        while (attacker.Length > 0 && defender.Length > 0)
        {
            char type = GetNextUnitToAttack(attacker, defender);

            if (CanCharSurviveSingleAttack(type, defender, out string removal))
                return true;
            else
                attacker = attacker.Remove(attacker.IndexOf(type), 1);

            foreach (char symbol in removal)
                defender = defender.Remove(defender.IndexOf(symbol), 1);
        }

        if (attacker == string.Empty)
            return false;

        if (defender == string.Empty)
            return true;

        return false;
    }

    public static char GetNextUnitToAttack(string from, string to)
    {
        if (from == string.Empty)
            throw new UnityException("from string is empty");

        // try find 2 weak enemies
        foreach (char symbol in from)
            if (to.Contains(GetWeakest(symbol).ToString() + GetWeakest(symbol).ToString()))
                return symbol;

        // try find at least 1 weak enemy
        foreach (char symbol in from)
            if (to.Contains(GetWeakest(symbol).ToString()))
                return symbol;

        // try find same enemy
        foreach (char symbol in from)
            if (to.Contains(symbol.ToString()))
                return symbol;

        // move in turn
        return from[0];
    }

    public static bool CanCharSurviveSingleAttack(char type, string defence, out string remove)
    {
        // at first attack the weakest enemy
        remove = string.Empty;
        int weakCount = 0;

        foreach (char unit in defence)
            if (unit == GetWeakest(type))
                weakCount++;

        switch (weakCount)
        {
            case 0:
                break;
            case 1:
                remove = GetWeakest(type).ToString();
                break;
            default:
                // two weak will destroy the attacking char
                remove = GetWeakest(type) + GetWeakest(type).ToString();
                return false;
        }

        // at second attack the same
        foreach (char unit in defence)
            if (unit == type)
            {
                // both will be destroyed
                remove += type;
                return false;
            }

        return remove.Length == defence.Length;
    }
}
