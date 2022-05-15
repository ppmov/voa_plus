using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeButton : Button
{
    protected override void Start()
    {
        onClick.AddListener(GoHome);
    }

    protected virtual void GoHome()
    {
        SceneManager.LoadScene("Start Menu");
    }
}
