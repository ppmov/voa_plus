using UnityEngine;
using UnityEngine.UI;
using Library;

public class ModeToggle : MonoBehaviour
{
    [SerializeField]
    private Image _single;
    [SerializeField]
    private Image _multi;
    [SerializeField]
    private TMPro.TMP_Text _text;

    private Slider _slider;

    public Mode Mode 
    { 
        get => _slider.value == 0 ? Mode.Single : Mode.Multi;
        set => _slider.value = value == Mode.Single ? 0 : 1;
    }

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _text.text = Mode.ToString();

        _single.color = Settings.GetColor(Player.Green);
        _multi.color = Settings.GetColor(Player.Red);
    }

    public void OnChangeMode()
    {
        _text.text = Mode.ToString();
        Storage.LastMode = Mode;
    }
}
