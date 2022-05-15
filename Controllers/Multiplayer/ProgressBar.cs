using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Library;

public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    private List<Dot> dots;

    public Player Player { get; set; } = Player.Green;

    private void OnEnable()
    {
        foreach (Dot dot in dots)
            if (dot.Player != Player.None)
                TakeDot(dot, Player.None);
    }

    public void Push()
    {
        foreach (Dot dot in dots)
        {
            if (dot.Player == Player.None)
            {
                TakeDot(dot, Player);
                return;
            }
            else
            if (dot.Player != Player)
                TakeDot(dot, Player);
        }
    }

    private static void TakeDot(Dot dot, Player player)
    {
        char unit = dot.Units[0];
        dot.Free();
        dot.Invade(unit, player);
    }
}
