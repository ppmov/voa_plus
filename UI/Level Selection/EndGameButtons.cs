using Library;
using UnityEngine;

public class EndGameButtons : MonoBehaviour
{
    [SerializeField]
    private GameObject restart;
    [SerializeField]
    private GameObject next;
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private LevelManager manager;

    public void Call(Player loser)
    {
        switch (loser)
        {
            case Player.Green:
                restart.SetActive(true);
                break;

            case Player.None:
                restart.SetActive(true);
                break;

            default:
                if (manager.CurrentLevel >= manager.GetLevelCount())
                    menu.SetActive(true);
                else
                    next.SetActive(true);
                break;
        }
    }

    public void HideAll()
    {
        restart.SetActive(false);
        next.SetActive(false);
        menu.SetActive(false);
    }
}
