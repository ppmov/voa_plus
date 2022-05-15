using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Library;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private ModeToggle modeSlider;
    [SerializeField]
    private Button playButton;

    private void Start()
    {
        Application.targetFrameRate = 60;
        modeSlider.Mode = Storage.LastMode;
    }

    public void OnPlay()
    {
        switch (modeSlider.Mode)
        {
            case Mode.Single:
                SceneManager.LoadScene("Singleplayer");
                break;
            case Mode.Multi:
                SceneManager.LoadScene("Multiplayer");
                break;
        }
    }
}